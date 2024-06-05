export function parseImageToMarkdown(blocks) {
    return `![${blocks.caption}](${blocks.file.url})`.concat('\n');
}