import { useState } from 'react';
import { Container, Row, Col, Alert, Card, Form } from 'react-bootstrap';
import axios from 'axios';
import Header from './components/Header/Header';
import JobDescriptionInput from './components/JobDescriptionInput/JobDescriptionInput';
import FormatSelector from './components/FormatSelector/FormatSelector';
import GenerateButton from './components/GenerateButton/GenerateButton';
import SuccessScreen from './components/SuccessScreen/SuccessScreen';
import TemplateSelector from './components/TemplateSelector/TemplateSelector';
import PageOptions from './components/PageOptions/PageOptions';
import MatchScoreCard from './components/MatchScore/MatchScoreCard';
import MissingSkillsCard from './components/MissingSkills/MissingSkillsCard';
import KeywordAnalysisCard from './components/KeywordAnalysis/KeywordAnalysisCard';
import ATSScoreCard from './components/ATSScore/ATSScoreCard';
import BulletEditor from './components/BulletEditor/BulletEditor';
import './styles/variables.css';
import './App.css';

const API_BASE_URL = (process.env.REACT_APP_API_URL || 'http://localhost:5000/api').replace(/\/$/, '');
const buildApiUrl = (path) => `${API_BASE_URL}${path.startsWith('/') ? path : `/${path}`}`;

function App() {
  // Form states
  const [jobDescription, setJobDescription] = useState('');
  const [selectedFormats, setSelectedFormats] = useState({ pdf: true, docx: false });
  const [template, setTemplate] = useState('harvard');
  const [allowTwoPages, setAllowTwoPages] = useState(false);

  // Flow states
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  //Results states
  const [analysisResult, setAnalysisResult] = useState(null);
  const [showBulletEditor, setShowBulletEditor] = useState(false);
  const [downloadSuccess, setDownloadSuccess] = useState(false);

  // Helpers
  const getFormats = () => {
    const formats = [];
    if (selectedFormats.pdf) formats.push('pdf');
    if (selectedFormats.docx) formats.push('docx');
    return formats;
  };

  const downloadFile = async (format) => {
    const response = await axios.get(
      buildApiUrl(`/cv/download/${format}`),
      { responseType: 'blob', withCredentials: true }
    );
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', response.headers['content-disposition']?.split('filename=')[1] || `CV.${format}`);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  };

  // Generate CV
  const handleGenerate = async () => {
    if (!jobDescription.trim()) { setError('Please paste a job description'); return; }
    if (!selectedFormats.pdf && !selectedFormats.docx) { setError('Please select at least one format'); return; }

    setError('');
    setLoading(true);
    setAnalysisResult(null);
    setDownloadSuccess(false);
    setShowBulletEditor(false);

    try {
      const response = await axios.post(
        buildApiUrl('/cv/generate'),
        {
          jobDescription,
          formats: getFormats(),
          template: template.charAt(0).toUpperCase() + template.slice(1),
          allowTwoPages
        },
        { withCredentials: true }
      );

      if (response.data.success) {
        // Download files
        for (const file of response.data.files) {
          await downloadFile(file.format);
        }

        // Save analysis results for display
        setAnalysisResult({
          matchScore: response.data.matchScore,
          missingSkills: response.data.missingSkills,
          keywordAnalysis: response.data.keywordAnalysis,
          atsScore: response.data.atsScore,
          generatedCV: response.data.generatedCV,
        });

        setDownloadSuccess(true);
        setLoading(false);
      } else {
        setError('The API did not return a successful generation response.');
        setLoading(false);
      }
    } catch (err) {
      if (!err.response) {
        setError(`Cannot reach API at ${API_BASE_URL}. Check REACT_APP_API_URL or start the backend on http://localhost:5000.`);
      } else {
        setError(err.response?.data?.error || 'An error occurred while generating the CV');
      }
      setLoading(false);
    }
  };

  // Handlers
  const handleFormatChange = (format) => {
    setSelectedFormats(prev => ({ ...prev, [format]: !prev[format] }));
  };

  const handleRegenerate = async (bulletSelections, editedCv) => {
    if (!analysisResult?.generatedCV) return;

    setLoading(true);
    setError('');

    try {
      const response = await axios.post(
        buildApiUrl('/cv/regenerate'),
        {
          generatedCV: editedCv || analysisResult.generatedCV,
          bulletSelections,
          formats: getFormats(),
          template: template.charAt(0).toUpperCase() + template.slice(1),
          allowTwoPages
        },
        { withCredentials: true }
      );

      if (response.data.success) {
        for (const file of response.data.files) {
          await downloadFile(file.format);
        }
        if (editedCv) {
          setAnalysisResult(prev => ({ ...prev, generatedCV: editedCv }));
        }
        setDownloadSuccess(true);
        setShowBulletEditor(false);
      }
    } catch (err) {
      setError(err.response?.data?.error || 'Error regenerating CV');
    } finally {
      setLoading(false);
    }
  };

  
  const handleReset = () => {
    setJobDescription('');
    setSelectedFormats({ pdf: true, docx: false });
    setTemplate('harvard');
    setAllowTwoPages(false);
    setAnalysisResult(null);
    setDownloadSuccess(false);
    setShowBulletEditor(false);
    setError('');

    document.getElementById('app-header')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  };

  // Rendering
  if (downloadSuccess && !analysisResult) {
    return <SuccessScreen onGoBack={handleReset} />;
  }

  return (
  <div className="app-container">
    <Container className="py-5">
      <Header />
      <Row>
        <Col lg={6} className="mb-4">
          <Card className="main-card">
            <Card.Body className="p-4">
              {error && (
                <Alert
                  variant="danger"
                  dismissible
                  onClose={() => setError('')}
                >
                  {error}
                </Alert>
              )}
              <Form>
                <h5 className="mb-4">Generate Your CV</h5>
                <JobDescriptionInput
                  value={jobDescription}
                  onChange={(e) => setJobDescription(e.target.value)}
                  disabled={loading}
                />
                <TemplateSelector
                  value={template}
                  onChange={setTemplate}
                  disabled={loading}
                />
                <Row>
                  <Col md={6}>
                    <FormatSelector
                      selectedFormats={selectedFormats}
                      onChange={handleFormatChange}
                      disabled={loading}
                    />
                  </Col>
                  <Col md={6}>
                    <PageOptions
                      allowTwoPages={allowTwoPages}
                      onChange={setAllowTwoPages}
                      disabled={loading}
                    />
                  </Col>
                </Row>
                <GenerateButton
                  loading={loading}
                  onClick={handleGenerate}
                  disabled={loading}
                />
              </Form>
            </Card.Body>
          </Card>
        </Col>
        <Col lg={6}>
          {analysisResult ? (
            <>
              <MatchScoreCard data={analysisResult.matchScore} />
              <ATSScoreCard data={analysisResult.atsScore} />
              <MissingSkillsCard data={analysisResult.missingSkills} />
              <KeywordAnalysisCard data={analysisResult.keywordAnalysis} />
              {!showBulletEditor && (
                <div className="text-center mb-4">
                  <button
                    className="btn btn-outline-primary me-3"
                    onClick={() => setShowBulletEditor(true)}
                  >
                    <i className="bi bi-pencil-square me-1" />
                    Edit Bullets & Regenerate
                  </button>

                  <button
                    className="btn btn-outline-secondary"
                    onClick={handleReset}
                  >
                    <i className="bi bi-arrow-counterclockwise me-1" />
                    Start Over
                  </button>
                </div>
              )}
              {showBulletEditor && (
                <BulletEditor
                  generatedCV={analysisResult.generatedCV}
                  onRegenerate={handleRegenerate}
                  onCancel={() => setShowBulletEditor(false)}
                  loading={loading}
                />
              )}
            </>
          ) : (
            <Card className="main-card">
              <Card.Body className="text-center text-muted">
                <p className="mb-0">
                  Generate results to see the analysis here
                </p>
              </Card.Body>
            </Card>
          )}
        </Col>
      </Row> 
    </Container>
    <div className="footer-note mt-4">
        <p>
          Your CV is generated based on a master profile and tailored to match
          the job requirements
        </p>
      </div>
  </div>
);
}

export default App;
