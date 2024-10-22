//TODO: Migrate to showdown or turndown
export function htmlToMarkdown(html) {
    const headerRegex = /<h([1-6])[^>]*>(.*?)<\/h\1>/g;
    const paragraphRegex = /<p[^>]*>(.*?)<\/p>/g;
    const linkRegex = /<a[^>]*href="([^"]+)"[^>]*>(.*?)<\/a>/g;
    const imageSrcRegex = /<img[^>]*src="([^"]+)"[^>]*>/g;
    const imageAltRegex = /<img[^>]*alt="([^"]+)"[^>]*>/g;
    const boldRegex = /<strong[^>]*>(.*?)<\/strong>/g;
    const italicRegex = /<em[^>]*>(.*?)<\/em>/g;
    const hrRegex = /<hr\s*.*?\/?>/g;
    const listItemRegex = /<li[^>]*>(.*?)<\/li>/g;
    const ulRegex = /<ul[^>]*>(.*?)<\/ul>/gs;
    const olRegex = /<ol[^>]*>(.*?)<\/ol>/gs;
    const metaCharsetRegex = /<meta\s+charset='utf-8'\s*\/?>/g;
    const emptyTagRegex = /<[^>]*>\s*<\/[^>]*>/g;
    const brRegex = /<br\s*.*?\/?>/g;
    const attributesRegex = /<[^>]*>/g;
    const inlineCodeRegex = /<code[^>]*>(.*?)<\/code>/gs;
    const codeRegex = /^(\s*)<code[^>]*>(.*?)<\/code>(\s*)\$/gm;

    function replaceHeaders(match, level, text) {
        return `${'#'.repeat(parseInt(level, 10))} ${text}`;
    }

    function replaceParagraphs(match, text) {
        return `\n${text}\n`;
    }

    function replaceLinks(match, url, text) {
        return ` [${text}](${url}) `;
    }

    function replaceImages(html) {
        let src = imageSrcRegex.exec(html);
        let alt = imageAltRegex.exec(html);
        src = src ? src[1] : '';
        alt = alt ? alt[1] : 'img';
        return `![${alt || src}](${src || alt})`;
    }

    function replaceBold(match, text) {
        return `**${text}**`;
    }

    function replaceItalic(match, text) {
        return `*${text}*`;
    }

    // eslint-disable-next-line
    function replaceHr(match) {
        return `\n---\n`;
    }

    function replaceListItems(match, text) {
        return `- ${text}`;
    }

    function replaceUl(match, text) {
        return `\n${text.replace(listItemRegex, replaceListItems)}\n`;
    }

    function replaceOl(match, text) {
        return `\n${text.replace(listItemRegex, replaceListItems)}\n`;
    }

    // eslint-disable-next-line
    function replaceBr(match) {
        return '  \n';
    }

    function removeAttributes(match) {
        return match.replace(/<[^>]*>/g, (tag) => {
            return tag.replace(/[^<>\s]+="[^"]*"/g, '');
        });
    }

    function replaceInlineCode(match, text) {
        return `\`${text}\``;
    }

    function replaceCode(match, leadingSpaces, text, trailingSpaces) {
        return `${leadingSpaces}\`\`\`\n\${text}\n\`\`\`${trailingSpaces}`;
    }

    let markdown = html
        .replace(headerRegex, replaceHeaders)
        .replace(paragraphRegex, replaceParagraphs)
        .replace(linkRegex, replaceLinks)
        .replace(/<img[^>]*\/?>/g, replaceImages)
        .replace(boldRegex, replaceBold)
        .replace(italicRegex, replaceItalic)
        .replace(hrRegex, replaceHr)
        .replace(ulRegex, replaceUl)
        .replace(olRegex, replaceOl)
        .replace(brRegex, replaceBr)
        .replace(metaCharsetRegex, '')
        .replace(emptyTagRegex, '')
        .replace(attributesRegex, removeAttributes)
        .replace(codeRegex, replaceCode)
        .replace(inlineCodeRegex, replaceInlineCode);

    return markdown.trim();
}