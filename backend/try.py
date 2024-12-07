import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart

# SMTP server configuration
smtp_server = "smtp.gmail.com"
smtp_port = 587
sender_email = "roofstockdev@gmail.com"
sender_password = "nplqhnvgsdysoxnf"
recipient_email = "kcanmersin@gmail.com"

# Email content
subject = "Test Email"
body = """
Hello,

This is a test email sent from a Python script.

Best regards,
Your Python Script
"""

try:
    # Create the email
    msg = MIMEMultipart()
    msg['From'] = sender_email
    msg['To'] = recipient_email
    msg['Subject'] = subject
    msg.attach(MIMEText(body, 'plain'))

    # Connect to the SMTP server
    server = smtplib.SMTP(smtp_server, smtp_port)
    server.starttls()  # Start TLS encryption
    server.login(sender_email, sender_password)

    # Send the email
    server.sendmail(sender_email, recipient_email, msg.as_string())
    print("Email sent successfully!")

except Exception as e:
    print(f"Failed to send email: {e}")

finally:
    # Close the connection
    server.quit()
