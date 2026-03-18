import { useCallback, useEffect, useState } from 'react';
import { Container, Row, Col, Alert, Card, Form } from 'react-bootstrap';
import axios from 'axios';
import Header from './components/Header/Header';
import JobDescriptionInput from './components/JobDescriptionInput/JobDescriptionInput';
import ProfileSelector from './components/ProfileSelector/ProfileSelector';
import FormatSelector from './components/FormatSelector/FormatSelector';
import GenerateButton from './components/GenerateButton/GenerateButton';
import SuccessScreen from './components/SuccessScreen/SuccessScreen';
import TemplateSelector from './components/TemplateSelector/TemplateSelector';
import PageOptions from './components/PageOptions/PageOptions';
import CVPreview from './components/CVPreview/CVPreview';
import HistoryPanel from './components/HistoryPanel/HistoryPanel';
import MatchScoreCard from './components/MatchScore/MatchScoreCard';
import MissingSkillsCard from './components/MissingSkills/MissingSkillsCard';
import KeywordAnalysisCard from './components/KeywordAnalysis/KeywordAnalysisCard';
import ATSScoreCard from './components/ATSScore/ATSScoreCard';
import BulletEditor from './components/BulletEditor/BulletEditor';
import { defaultSettings, loadSettings, saveSettings } from './utils/settings';
import { clearHistory, getHistory, saveToHistory } from './utils/history';
import './styles/variables.css';
import './App.css';

const API_BASE_URL = (process.env.REACT_APP_API_URL || 'http://localhost:5000/api').replace(/\/$/, '');
const buildApiUrl = (path) => `${API_BASE_URL}${path.startsWith('/') ? path : `/${path}`}`;
const initialSettings = loadSettings();

function App() {
  const [jobDescription, setJobDescription] = useState('');
  const [selectedFormats, setSelectedFormats] = useState(initialSettings.selectedFormats);
  const [template, setTemplate] = useState(initialSettings.template);
  const [allowTwoPages, setAllowTwoPages] = useState(initialSettings.allowTwoPages);
  const [profiles, setProfiles] = useState([{ id: 'cv-master', name: 'Default' }]);
  const [selectedProfileId, setSelectedProfileId] = useState('cv-master');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [analysisResult, setAnalysisResult] = useState(null);
  const [showBulletEditor, setShowBulletEditor] = useState(false);
  const [downloadSuccess, setDownloadSuccess] = useState(false);
  const [historyEntries, setHistoryEntries] = useState([]);
  const [showHistoryPanel, setShowHistoryPanel] = useState(false);

  useEffect(() => {
    let mounted = true;

    const fetchProfiles = async () => {
      try {
        const response = await axios.get(buildApiUrl('/cv/profiles'));
        if (!mounted || !Array.isArray(response.data) || response.data.length === 0) return;

        setProfiles(response.data);
        setSelectedProfileId((currentId) =>
          response.data.some((profile) => profile.id === currentId)
            ? currentId
            : response.data[0].id
        );
      } catch {
      }
    };

    fetchProfiles();

    return () => {
      mounted = false;
    };
  }, []);

  useEffect(() => {
    setHistoryEntries(getHistory());
  }, []);

  useEffect(() => {
    saveSettings({
      template,
      selectedFormats,
      allowTwoPages,
    });
  }, [template, selectedFormats, allowTwoPages]);
  const getFormats = useCallback(() => {
    const formats = [];
    if (selectedFormats.pdf) formats.push('pdf');
    if (selectedFormats.docx) formats.push('docx');
    return formats;
  }, [selectedFormats]);

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
  const handleGenerate = useCallback(async () => {
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
          allowTwoPages,
          profileId: selectedProfileId,
        },
        { withCredentials: true }
      );

      if (response.data.success) {
        for (const file of response.data.files) {
          await downloadFile(file.format);
        }
        setAnalysisResult({
          matchScore: response.data.matchScore,
          missingSkills: response.data.missingSkills,
          keywordAnalysis: response.data.keywordAnalysis,
          atsScore: response.data.atsScore,
          generatedCV: response.data.generatedCV,
          previewHtml: response.data.previewHtml,
        });

        const updatedHistory = saveToHistory({
          jobDescription,
          matchScore: response.data.matchScore?.overall,
          template: template.charAt(0).toUpperCase() + template.slice(1),
        });
        setHistoryEntries(updatedHistory);

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
  }, [allowTwoPages, getFormats, jobDescription, selectedFormats, selectedProfileId, template]);

  useEffect(() => {
    const handleKeydown = async (event) => {
      if (event.repeat) return;

      const isPasteShortcut =
        (event.ctrlKey || event.metaKey) &&
        event.shiftKey &&
        event.key.toLowerCase() === 'v';

      if (isPasteShortcut && !loading) {
        event.preventDefault();
        try {
          const text = await navigator.clipboard.readText();
          if (text) setJobDescription(text);
        } catch {
        }
        return;
      }

      const isGenerateShortcut =
        (event.ctrlKey || event.metaKey) &&
        !event.shiftKey &&
        event.key === 'Enter';

      if (isGenerateShortcut && !loading && jobDescription.trim()) {
        event.preventDefault();
        handleGenerate();
      }
    };

    window.addEventListener('keydown', handleKeydown);
    return () => window.removeEventListener('keydown', handleKeydown);
  }, [handleGenerate, jobDescription, loading]);
  const handleFormatChange = (format) => {
    setSelectedFormats(prev => ({ ...prev, [format]: !prev[format] }));
  };

  const handleLoadHistoryEntry = (entry) => {
    setJobDescription(entry.jobDescription || '');
    setShowHistoryPanel(false);
    setError('');
    document.getElementById('app-header')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  };

  const handleClearHistory = () => {
    clearHistory();
    setHistoryEntries([]);
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
        setAnalysisResult((prev) => ({
          ...prev,
          generatedCV: editedCv || prev.generatedCV,
          previewHtml: response.data.previewHtml ?? prev.previewHtml,
        }));
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
    setSelectedFormats({ ...defaultSettings.selectedFormats });
    setTemplate(defaultSettings.template);
    setAllowTwoPages(defaultSettings.allowTwoPages);
    setAnalysisResult(null);
    setDownloadSuccess(false);
    setShowBulletEditor(false);
    setError('');

    document.getElementById('app-header')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  };
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
                <div className="form-title-row mb-4">
                  <h5 className="mb-0">Generate Your CV</h5>
                  <button
                    type="button"
                    className="btn btn-outline-secondary btn-sm"
                    onClick={() => setShowHistoryPanel((prev) => !prev)}
                    disabled={loading}
                  >
                    {showHistoryPanel ? 'Hide History' : 'History'}
                  </button>
                </div>
                <ProfileSelector
                  profiles={profiles}
                  value={selectedProfileId}
                  onChange={setSelectedProfileId}
                  disabled={loading}
                />
                <JobDescriptionInput
                  value={jobDescription}
                  onChange={(e) => setJobDescription(e.target.value)}
                  disabled={loading}
                  helperText="Shortcuts: Ctrl/Cmd+Shift+V paste, Ctrl/Cmd+Enter generate"
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
              {showHistoryPanel && (
                <HistoryPanel
                  entries={historyEntries}
                  onLoad={handleLoadHistoryEntry}
                  onClear={handleClearHistory}
                  onClose={() => setShowHistoryPanel(false)}
                  disabled={loading}
                />
              )}
            </Card.Body>
          </Card>
        </Col>
        <Col lg={6}>
          {analysisResult ? (
            <>
              <CVPreview html={analysisResult.previewHtml} />
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
