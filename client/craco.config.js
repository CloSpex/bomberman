const path = require('path');

module.exports = {
  webpack: {
    alias: {
      '@enums': path.resolve(__dirname, 'src/common/enums'),
      '@interfaces': path.resolve(__dirname, 'src/common/interfaces'),
      '@sharedComponents': path.resolve(__dirname, 'src/common/components'),
    },
  },
};
