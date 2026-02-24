module.exports = {
  default: {
    requireModule: ['ts-node/register'],
    require: ['tests/e2e/**/*.steps.ts'],
    paths: ['tests/e2e/**/*.feature'],
    format: ['progress', 'html:test-results/cucumber-report.html'],
    publishQuiet: true,
  },
};
