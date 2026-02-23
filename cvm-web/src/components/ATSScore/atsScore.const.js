export const ATS_STATUS = {
  OK: 'green',
  WARNING: 'yellow',
  RISK: 'red',
};

export const DEFAULT_ATS_STATUS = ATS_STATUS.WARNING;

export const ATS_STATUS_LABEL = {
  [ATS_STATUS.OK]: 'ATS Ready',
  [ATS_STATUS.WARNING]: 'Needs Attention',
  [ATS_STATUS.RISK]: 'ATS Risk',
};

export const ATS_SCORE_MIN = 0;
export const ATS_SCORE_MAX = 100;
