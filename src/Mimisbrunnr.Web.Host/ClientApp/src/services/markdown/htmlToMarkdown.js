import TurndownService from 'turndown';

const turndownService = new TurndownService({
  headingStyle: 'atx',
  codeBlockStyle: 'fenced',
  bulletListMarker: '-',
  emDelimiter: '*',
  strongDelimiter: '**',
  linkStyle: 'inlined',
});

turndownService.addRule('strikethrough', {
  filter: ['del', 's', 'strike'],
  replacement: function(content) {
    return '~~' + content + '~~';
  }
});

turndownService.addRule('table', {
  filter: 'table',
  replacement: function(content, node) {
    const rows = node.querySelectorAll('tr');
    if (rows.length === 0) return content;
    
    let md = '';
    
    rows.forEach((row, rowIndex) => {
      const cells = row.querySelectorAll('th, td');
      const cellContents = Array.from(cells).map(cell => {
        let cellText = cell.textContent.trim();
        cellText = cellText.replace(/\n/g, ' ');
        return cellText;
      });
      
      md += '| ' + cellContents.join(' | ') + ' |\n';
      
      if (rowIndex === 0) {
        const separators = cells.length;
        md += '| ' + Array(separators).fill('---').join(' | ') + ' |\n';
      }
    });
    
    return '\n' + md + '\n';
  }
});

turndownService.addRule('taskListItems', {
  filter: function(node) {
    return node.type === 'checkbox' && node.parentElement && node.parentElement.nodeName === 'LI';
  },
  replacement: function(content, node) {
    const checked = node.checked ? '[x]' : '[ ]';
    return checked + ' ';
  }
});

turndownService.addRule('remove invisible', {
  filter: function(node) {
    return node.style.display === 'none' || 
           node.style.visibility === 'hidden' ||
           (node.style.width === '0px' && node.style.height === '0px');
  },
  replacement: function() {
    return '';
  }
});

turndownService.addRule('image', {
  filter: 'img',
  replacement: function(content, node) {
    const alt = node.alt || '';
    const src = node.src || '';
    if (!src) return '';
    return `![${alt}](${src})`;
  }
});

export function htmlToMarkdown(html) {
  if (!html || typeof html !== 'string' || html.trim() === '') {
    return '';
  }
  
  try {
    const markdown = turndownService.turndown(html);
    return markdown;
  } catch (error) {
    console.error('HTML to Markdown conversion error:', error);
    return null;
  }
}

export function detectAndConvertToMarkdown(text, html) {
  if (html && html.trim()) {
    const markdown = htmlToMarkdown(html);
    if (markdown && markdown.trim()) {
      return markdown;
    }
  }
  
  if (text && text.trim()) {
    return text;
  }
  
  return null;
}
