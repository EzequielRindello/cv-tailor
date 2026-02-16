import React, { useState } from 'react';
import { Container, Row, Col, Alert, Card, Form } from 'react-bootstrap';
import axios from 'axios';
import Header from './components/Header/Header';
import JobDescriptionInput from './components/JobDescriptionInput/JobDescriptionInput';
import FormatSelector from './components/FormatSelector/FormatSelector';
import GenerateButton from './components/GenerateButton/GenerateButton';
import SuccessScreen from './components/SuccessScreen/SuccessScreen';
import './styles/variables.css';
import './styles/reset.css';
import './App.css';

const API_BASE_URL = process.env.REACT_APP_API_URL;

function App() {
  const [jobDescription, setJobDescription] = useState('');
  const [selectedFormats, setSelectedFormats] = useState({ pdf: true, docx: false });
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  const handleFormatChange = (format) => {
    setSelectedFormats(prev => ({
      ...prev,
      [format]: !prev[format]
    }));
  };

  const handleGenerate = async () => {
    if (!jobDescription.trim()) {
      setError('Please paste a job description');
      return;
    }

    if (!selectedFormats.pdf && !selectedFormats.docx) {
      setError('Please select at least one format');
      return;
    }

    setError('');
    setLoading(true);
    setSuccess(false);

    try {
      const formats = [];
      if (selectedFormats.pdf) formats.push('pdf');
      if (selectedFormats.docx) formats.push('docx');

      const response = await axios.post(
        `${API_BASE_URL}/cv/generate`,
        { jobDescription, formats },
        { withCredentials: true }
      );

      if (response.data.success) {
        for (const file of response.data.files) {
          await downloadFile(file.format);
        }
        setSuccess(true);
        setLoading(false);
      }
    } catch (err) {
      setError(err.response?.data?.error || 'An error occurred while generating the CV');
      setLoading(false);
    }
  };

  const downloadFile = async (format) => {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/cv/download/${format}`,
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
    } catch (err) {
      console.error('Download error:', err);
    }
  };

  const handleGoBack = () => {
    setJobDescription('');
    setSelectedFormats({ pdf: true, docx: false });
    setSuccess(false);
    setError('');
  };

  if (success) {
    return <SuccessScreen onGoBack={handleGoBack} />;
  }

  return (
    <div className="app-container">
      <Container className="py-5">
        <Row className="justify-content-center">
          <Col lg={8} xl={7}>
            <Header />

            <Card className="main-card">
              <Card.Body className="p-4 p-md-5">
                {error && (
                  <Alert variant="danger" dismissible onClose={() => setError('')}>
                    {error}
                  </Alert>
                )}

                <Form>
                  <JobDescriptionInput
                    value={jobDescription}
                    onChange={(e) => setJobDescription(e.target.value)}
                    disabled={loading}
                  />

                  <FormatSelector
                    selectedFormats={selectedFormats}
                    onChange={handleFormatChange}
                    disabled={loading}
                  />

                  <GenerateButton
                    loading={loading}
                    onClick={handleGenerate}
                    disabled={loading}
                  />
                </Form>
              </Card.Body>
            </Card>

            <div className="footer-note">
              <p>
                Your CV is generated based on a master profile and tailored to match the job requirements
              </p>
            </div>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

export default App;