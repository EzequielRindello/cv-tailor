import React from 'react';
import { Container, Row, Col, Card, Button } from 'react-bootstrap';
import './SuccessScreen.css';

const SuccessScreen = ({ onGoBack }) => {
  return (
    <div className="success-screen">
      <Container>
        <Row className="justify-content-center align-items-center min-vh-100">
          <Col md={6} lg={5}>
            <Card className="success-card">
              <Card.Body className="text-center p-5">
                <div className="success-icon mb-4">
                  <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                    <polyline points="22 4 12 14.01 9 11.01"></polyline>
                  </svg>
                </div>
                <h2 className="mb-3">CV Generated Successfully!</h2>
                <p className="text-muted mb-4">
                  Your customized CV has been downloaded to your device.
                </p>
                <Button
                  variant="primary"
                  size="lg"
                  onClick={onGoBack}
                  className="success-btn px-5"
                >
                  Generate Another CV
                </Button>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </Container>
    </div>
  );
};

export default SuccessScreen;