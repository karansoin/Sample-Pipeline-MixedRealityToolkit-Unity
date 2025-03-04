import socket
import json
import hl2ss
print(dir(hl2ss))

# Server settings
HOST = "0.0.0.0"  # Listen on all interfaces
PORT = 5005  # Same port as Unity

# HoloLens 2 settings
HOLOLENS_IP = "192.168.1.40"
HL2_PORT = hl2ss.IPCPort.UNITY_MESSAGE_QUEUE  # HL2ss message queue port

def start_server():
    # Create a TCP/IP socket
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen(5)

    print(f"✅ Server is listening on {HOST}:{PORT}")

    while True:
        conn, addr = server_socket.accept()
        print(f"🔵 Connection from: {addr}")

        try:
            # Receive data
            data = conn.recv(1024).decode('utf-8')
            if not data:
                continue

            # Parse JSON
            message = json.loads(data)
            print(f"📩 Received Data: {message}")

            # Forward to HoloLens
            send_to_hololens(message)

        except Exception as e:
            print(f"❌ Error: {e}")

        finally:
            conn.close()

def send_to_hololens(data):
    try:
        client = hl2ss.ipc_umq(HOLOLENS_IP, HL2_PORT)
        client.open()

        # Convert data to byte format
        json_data = json.dumps(data).encode("utf-8")
        buffer = hl2ss.umq_command_buffer()
        buffer.add(1, json_data)  # ID = 1 for test message

        # Send data to HL2
        client.push(buffer)
        print("✅ Data forwarded to HoloLens 2")

        client.close()
    except Exception as e:
        print(f"❌ Failed to send data to HL2: {e}")

if __name__ == "__main__":
    start_server()


# import socket
# import json
# import signal
# import sys

# def shutdown_server(signal, frame):
#     print("\n🛑 Server is shutting down...")
#     server.close()
#     sys.exit(0)

# signal.signal(signal.SIGINT, shutdown_server)

# HOST = "0.0.0.0"  # Change this to "0.0.0.0" if receiving from another device
# PORT = 5005         # Must match Unity's port

# server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
# server.bind((HOST, PORT))
# server.listen(5)

# print(f"🔵 Server is listening on {HOST}:{PORT}...")

# while True:
#     try:
#         conn, addr = server.accept()
#         print(f"✅ Connected to {addr}")

#         data = conn.recv(1024).decode()
#         if not data:
#             continue

#         try:
#             received_json = json.loads(data)
#             print(f"📩 Received JSON: {received_json}")
#         except json.JSONDecodeError:
#             print(f"⚠️ Received (not JSON): {data}")

#         conn.close()
#     except KeyboardInterrupt:
#         shutdown_server(None, None)
# # while True:
# #     conn, addr = server.accept()
# #     print(f"✅ Connected to {addr}")

# #     data = conn.recv(1024).decode()  # Receive and decode message
# #     if not data:
# #         continue

# #     if data.strip().lower() == "exit":  # If "exit" message is received, stop
# #         print("🛑 Shutdown command received. Closing server...")
# #         break
    
# #     try:
# #         received_json = json.loads(data)  # Convert to JSON
# #         print(f"📩 Received JSON: {received_json}")  # Print to console
# #     except json.JSONDecodeError:
# #         print(f"⚠️ Received (not JSON): {data}")

# #     conn.close()
# #     except KeyboardInterrupt:
# #         shutdown_server(None, None)

# server.close()
