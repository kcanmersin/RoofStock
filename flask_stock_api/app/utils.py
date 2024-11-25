import json

def validate_request(data, required_fields):
    """Validate incoming request data."""
    for field in required_fields:
        if field not in data:
            return False, f"Missing required field: {field}"
    return True, ""
