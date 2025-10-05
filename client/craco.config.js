const path = require('path');

module.exports = {
  webpack: {
    alias: {
      '@enums': path.resolve(__dirname, 'src/enums'),
      '@interfaces': path.resolve(__dirname, 'src/interfaces'),
    },
  },
};
