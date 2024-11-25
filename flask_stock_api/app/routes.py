from flask import Blueprint, request, jsonify
from .workflows import execute_workflow

# Create a Blueprint
bp = Blueprint('main', __name__)

@bp.route('/api/stock', methods=['POST'])
def handle_stock_action():
    data = request.json
    action = data.get('action', '')
    user_id = data.get('user_id', '')

    # Execute the workflow
    response = execute_workflow(action, user_id)
    return jsonify(response)
