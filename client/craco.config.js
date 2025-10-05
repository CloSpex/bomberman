const path = require("path");
const tsconfig = require("./tsconfig.json");

const aliasMap = {};
if (tsconfig.compilerOptions && tsconfig.compilerOptions.paths) {
  const paths = tsconfig.compilerOptions.paths;
  for (const [alias, targets] of Object.entries(paths)) {
    const cleanAlias = alias.replace("/*", "");
    const target = targets[0].replace("/*", "");
    aliasMap[cleanAlias] = path.resolve(
      __dirname,
      tsconfig.compilerOptions.baseUrl,
      target,
    );
  }
}

module.exports = {
  webpack: {
    alias: aliasMap,
  },
  style: {
    postcss: {
      mode: "file", // force CRACO to respect plugins
      plugins: [require("@tailwindcss/postcss"), require("autoprefixer")],
    },
  },
};
