import { remark } from "remark";
import remarkGfm from 'remark-gfm';
import { parseMarkdownToHeader } from './parsers/HeaderTypeParser';
import { parseMarkdownToParagraph } from './parsers/ParagraphTypeParser';
import { parseMarkdownToList } from './parsers/ListTypeParser';
import { parseMarkdownToDelimiter } from './parsers/DelimiterTypeParser';
import { parseMarkdownToCode } from '@/services/editorJs/parsers/CodeTypeParser';
import { parseMarkdownToQuote } from './parsers/QuoteTypeParser';
import { parseMarkdownToTable } from './parsers/TableTypeParser';

import { parseHeaderToMarkdown } from './parsers/HeaderTypeParser';
import { parseParagraphToMarkdown } from './parsers/ParagraphTypeParser';
import { parseListToMarkdown } from './parsers/ListTypeParser';
import { parseDelimiterToMarkdown } from './parsers/DelimiterTypeParser';
import { parseImageToMarkdown } from './parsers/ImageTypeParser';
import { parseCheckboxToMarkdown } from './parsers/CheckboxTypeParser';
import { parseQuoteToMarkdown } from './parsers/QuoteTypeParser';
import { parseCodeToMarkdown } from './parsers/CodeTypeParser';
import { parseTableToMarkdown } from './parsers/TableTypeParser';


export function parseMarkdownToEditorJs(markdown) {
    const editorData = [];
    const parsedMarkdown = remark()
                            .use(remarkGfm)
                            .parse(markdown);
    // iterating over the pared remarkjs syntax tree and executing the json parsers
    // eslint-disable-next-line
    parsedMarkdown.children.forEach((item, index) => {
        switch (item.type) {
            case 'heading':
                return editorData.push(parseMarkdownToHeader(item));
            case 'paragraph':
                var paragraphs = parseMarkdownToParagraph(item);
                return paragraphs.forEach(p => editorData.push(p));
            case 'list':
                return editorData.push(parseMarkdownToList(item));
            case 'thematicBreak':
                return editorData.push(parseMarkdownToDelimiter());
            case 'code':
                return editorData.push(parseMarkdownToCode(item));
            case 'blockquote':
                return editorData.push(parseMarkdownToQuote(item));
            case 'table':
                return editorData.push(parseMarkdownToTable(item));
            default:
                break;
        }

    });

    return editorData;
}

export function parseEditorJsToMarkdown(data) {
    const initialData = {};
    initialData.content = data.blocks;

    const parsedData = initialData.content.map((item) => {
        // iterate through editor data and parse the single blocks to markdown syntax
        switch (item.type) {
            case 'header':
                return parseHeaderToMarkdown(item.data);
            case 'paragraph':
                return parseParagraphToMarkdown(item.data);
            case 'list':
                return wrapWithNewLine(parseListToMarkdown(item.data));
            case 'delimiter':
                return parseDelimiterToMarkdown(item);
            case 'image':
                return parseImageToMarkdown(item.data);
            case 'quote':
                return parseQuoteToMarkdown(item.data);
            case 'checkbox':
                return wrapWithNewLine(parseCheckboxToMarkdown(item.data));
            case 'code':
                return parseCodeToMarkdown(item.data);
            case 'checklist':
                return wrapWithNewLine(parseCheckboxToMarkdown(item.data));
            case 'table':
                return wrapWithNewLine(parseTableToMarkdown(item.data));
            default:
                break;
        }
    });

    return parsedData.join('\n');
}

function wrapWithNewLine(result) {
    return `\n${result}\n`;
}