from flask import Flask, request, jsonify
import html
import json

app = Flask(__name__)

@app.route('/render', methods=['POST'])
def render_macro():
    data = request.json
    print(json.dumps(data))

    # Формируем HTML
    html_content = f"""
    <div>
        <style>
            body {{ font-family: Arial, sans-serif; margin: 20px; }}
            .container {{ border: none; padding: 20px; border-radius: 5px; max-width: 800px; }}
            .header {{ background-color: #f5f5f5; padding: 10px; margin-bottom: 20px; border-radius: 5px; }}
            .params-table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
            .params-table th, .params-table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
            .params-table th {{ background-color: #f5f5f5; }}
        </style>
        <div class="container">
            <div class="header">
                <h2>Macro Render Request</h2>
            </div>
            <p><strong>Plugin Identifier:</strong> {data.get('pluginIdentifier', '')}</p>
            <p><strong>Macro Identifier:</strong> {data.get('macroIdentifier', '')}</p>
            <p><strong>Requested By:</strong> {data.get('requestedBy', {}).get('name', '')} ({data.get('requestedBy', {}).get('email', '')})</p>
            <p><strong>Page ID:</strong> {data.get('pageId', '')}</p>
            <p><strong>Space Key:</strong> {data.get('spaceKey', '')}</p>
            <p><strong>User Token:</strong> {data.get('userToken', '') or 'Not provided'}</p>

            <h3>Parameters:</h3>
            <table class="params-table">
                <thead>
                    <tr>
                        <th>Key</th>
                        <th>Value</th>
                    </tr>
                </thead>
                <tbody>
    """

    params = data.get('params', {})
    for key, value in params.items():
        html_content += f"""
                    <tr>
                        <td>{key}</td>
                        <td>{value}</td>
                    </tr>
        """

    html_content += """
                </tbody>
            </table>
        </div>
    </div>
    """
    disable_inline = "<style>div[aria-name=\"mimisbrunnr.plugin.example.macro.external-renderer\"] { display: block!important;}</style>"

    # Экранируем кавычки для корректного отображения в srcdoc
    escaped_html = html.escape(html_content)
    result_html = f'<br/><iframe srcdoc="{escaped_html}" style="width: 100%; height: 800px; border: none;"></iframe>{disable_inline}'
    return jsonify({'Html': result_html})

if __name__ == '__main__':
    app.run("0.0.0.0", 27080, debug=True)
