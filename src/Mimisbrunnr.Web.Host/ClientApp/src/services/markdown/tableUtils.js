"use strict";
/**
 *  Format tables in markdown text
 * @param {string} text 
 * @returns {string} formatted markdown
 */
export function formatMarkdownTables(text) {
    // Поиск всех таблиц формата Markdown в тексте
    var regex = /(?:\|.*\|)\n(?:.*\|.*\n)*(?:.*\|.*)?/g;
    var tables = text.match(regex);

    // Форматирование каждой таблицы
    for (var i = 0; i < tables.length; i++) {
        var formattedTable = formatMarkdownTable(tables[i]);
        text = text.replace(tables[i], formattedTable);
    }

    return text;
}

/**
 *  Format a markdown table
 * @param {string} table 
 * @returns {string} formatted table
 */
export function formatMarkdownTable(table) {
    // Get table lines
    var rows = table.split("\n");

    // Get column indexes
    var cols = [];
    var header = rows[0].split("|");
    for (var i = 1; i < header.length - 1; i++) {
        if (header[i].trim() !== "")
            cols.push(i);
    }

    // Get max with in every column
    var maxWidths = [];
    for (var i = 0; i < cols.length; i++) {
        var widths = [];
        for (var j = 0; j < rows.length; j++) {
            var cell = rows[j].split("|")[cols[i]];
            var length = cell === undefined ? 0 : cell.length;
            widths.push(length);
        }
        maxWidths.push(Math.max.apply(null, widths));
    }

    // Formatting table
    var i = 0; //fix when indexers not set to zero
    var j = 0; //fix when indexers not set to zero
    var k = 0; //fix when indexers not set to zero
    var formattedTable = "";
    for (var i = 0; i < rows.length; i++) {
        var cells = rows[i].split("|");
        if (cells.length === 0 || (cells.length === 1 && cells[0] === "")) 
            continue;
        formattedTable += "|";
        for (var j = 0; j < cols.length; j++) {
            var cell = cells[cols[j]];
            var length = cell === undefined ? 0 : cell.length;
            var padding = maxWidths[j] - length;
            for (var k = 0; k < padding; k++) {
                cell += "\t";
            }
            k = 0;
            formattedTable += cell + "|";
        }
        j = 0;
        formattedTable += "\n";
    }

    return formattedTable;
}

/**
 * Inserting new column into table
 * @param {CodeMirror} cm 
 */
export function insertMarkdownTableColumn(cm) {
    const cursor = cm.getCursor();

    // Find table where cursor is
    const regex = /(?:\|.*\|)\n(?:.*\|.*\n)*(?:.*\|.*)?/g;
    const tables = cm.getValue().match(regex).reverse();
    let tableStart = -1;
    let lines = undefined;

    for(let i = 0; i < tables.length; i++) {
        let table = tables[i];
        if(table === "") 
            continue;

        // Get table position
        lines = table.split("\n");
        if (cursor.line === 0 && cm.getLine(cursor.line) != lines[0])
            continue;

        for(let i = cursor.line; i >= 0; i--){
            if(cm.getLine(i) != lines[0])
                continue;
            tableStart = i;
        }

        if(tableStart !== -1)
            break;
    }

    if (tableStart == -1)
        return;

    let newTable = "";
    for (let i = 0; i < lines.length -1; i++) {
        switch(i){
            case 0:
                newTable += lines[i] + " new column |\n";
                break;
            case 1:
                newTable += lines[i] + "------------|\n";
                break;
            default:
                newTable += lines[i] + "\t\t\t\t\t\t\t\t\t\t\t\t|\n";
        }
    }
    cm.replaceRange(newTable, { ch: 0, line: tableStart }, { ch: 0, line: tableStart + lines.length - 1});
    cm.setCursor({ ch: newTable.split("\n")[0].length, line: tableStart });
}
  
/**
 *  Inserting a new table row in codemirror
 * @param {CodeMirror} cm 
 */
export function insertMarkdownTableRow(cm) {
    const cursor = cm.getCursor();

    // Find table where cursor is
    const regex = /(?:\|.*\|)\n(?:.*\|.*\n)*(?:.*\|.*)?/g;
    const match = regex.exec(cm.getValue());
    const table = match ? match[0] : "";
    const start = match ? match.index : 0;

    let row = 0;
    const lines = table.split("\n");
    for (let i = 0; i < lines.length; i++) {
        if (start + lines[i].length <= cursor.ch) {
            row = i + 1;
        }
    }

    if (row === 0)
        return;

    let newTable = "";
    for (let i = 0; i < lines.length; i++) {
        newTable += lines[i] + "\n";
        if (i === row - 1) {
            const cells = lines[i].split("|");
            newTable += "|";
            for (let j = 1; j < cells.length - 1; j++) {
            newTable += cells[j].indexOf("\n") === -1 ? " " : "" + "|";
            }
            newTable += "\n";
        }
    }
    cm.replaceRange(newTable, { ch: 0, line: row - 1 }, { ch: 0, line: row });
    cm.setCursor({ ch: 0, line: row });
}