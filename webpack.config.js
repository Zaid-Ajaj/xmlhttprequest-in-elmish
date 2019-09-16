var path = require("path");

module.exports = {
    mode: "none",
    entry: "./src/App.fsproj",
    devServer: {
        contentBase: path.resolve(__dirname, "./dist"),
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }]
    }
}