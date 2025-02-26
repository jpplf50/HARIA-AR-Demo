import socket
import time
import random
import threading

# Safety words to choose from
safetyList = ["safe", "caution", "danger"] 

# Global variables
battery_level = 100  # Starting battery level
stop_client = False  # Flag to stop the client

def send_messages(client_socket):
    global battery_level, stop_client

    while not stop_client:
        # Generate random grip strength (0-100)
        grip_strength = random.randint(0, 100)

        # Randomly choose an activity
        activities = ["planning", "calculating", "moving"]
        activity = random.choice(activities)

        # Randomly choose an emoji
        safety = random.choice(safetyList)

        # Create the message
        message = f"Grip Strength: {grip_strength}%, Currently: {activity}, Battery: {battery_level}%, {safety}"

        # Send the message to the server
        client_socket.send(message.encode("utf-8"))
        print(f"Sent: {message}")

        # Decrease battery level by 1% every 2 seconds
        time.sleep(2)
        battery_level = max(0, battery_level - 1)  # Ensure battery doesn't go below 0

        # Wait for 1 second before sending the next message
        time.sleep(1)

def start_client(server_ip, server_port):
    global stop_client

    # Create a TCP socket
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    try:
        # Connect to the server
        client_socket.connect((server_ip, server_port))
        print(f"Connected to server at {server_ip}:{server_port}")

        # Start a thread to send messages
        threading.Thread(target=send_messages, args=(client_socket,), daemon=True).start()

        # Wait for the user to type "quit"
        while True:
            user_input = input("Type 'quit' to stop the client: ")
            if user_input.lower() == "quit":
                stop_client = True
                print("Stopping client...")
                break

    except Exception as e:
        print(f"Error: {e}")
    finally:
        # Close the socket
        client_socket.close()
        print("Connection closed.")

if __name__ == "__main__":
    # Replace with your server's IP and port
    SERVER_IP = "127.0.0.1"  # Localhost
    SERVER_PORT = 5000       # Port used in Unity TCP server

    start_client(SERVER_IP, SERVER_PORT)