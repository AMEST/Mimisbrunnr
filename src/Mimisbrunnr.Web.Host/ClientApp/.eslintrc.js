module.exports = {
  root: true,
  env: {
    node: true
  },
  'extends': [
    'plugin:vue/essential',
    'eslint:recommended'
  ],
  rules: {
    'no-console': 'off',
    'no-debugger': 'off',
    'vue/multi-word-component-names': 'off',
    'vue/no-reserved-component-names': 'off',
    'vue/no-mutating-props': 'off',
    'no-prototype-builtins': 'off'
  },
  parserOptions: {
    parser: '@babel/eslint-parser'
  }
}
