import {parseTextFromMarkdown, replaceTextModifiersHtmlToMarkdown} from './TextParser';

export function parseParagraphToMarkdown(blocks) {
    return `${replaceTextModifiersHtmlToMarkdown(blocks.text)}\n`;
}

export function parseMarkdownToParagraph(blocks) {
    return parseTextFromMarkdown(blocks);
}