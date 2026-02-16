import React from 'react';
import { Button, Spinner } from 'react-bootstrap';
import './GenerateButton.css';

const GenerateButton = ({ loading, onClick, disabled }) => {
  return (
    <Button
      variant="primary"
      size="lg"
      className="generate-btn"
      onClick={onClick}
      disabled={disabled || loading}
    >
      {loading ? (
        <>
          <Spinner
            as="span"
            animation="border"
            size="sm"
            role="status"
            className="me-2"
          />
          Generating Your CV...
        </>
      ) : (
        'Generate CV'
      )}
    </Button>
  );
};

export default GenerateButton;