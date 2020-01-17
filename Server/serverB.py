import socket
import threading
import sys
import json
import tongbuB

my_ip = sys.argv[1]
my_port = int(sys.argv[2])
tongbu_server, tongbu_port, tongbu_type = '', 0, 'B'
my_type = 'B'
if len(sys.argv) > 3:
    tongbu_server, tongbu_port, tongbu_type = sys.argv[3], int(sys.argv[4]), sys.argv[5]


def main():
    tongbuB.init(tongbu_server, tongbu_port)
    start_server()


def self_as_peer():
    return {'ip': my_ip, 'port': my_port, 'type': my_type, 'alive': True}


def start_server():
    ss = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    ss.bind((my_ip, my_port))
    # ss.bind((socket.gethostname(), my_port))
    ss.listen(5)
    print('服务B启动')
    while True:
        conn, address = ss.accept()
        print('监听到消息' + str(address))
        thread = threading.Thread(target=handle_connection, args=(conn, address))
        thread.start()


def handle_connection(conn, address):
    data = str(conn.recv(1024).decode('UTF-8'))
    js = json.loads(data)
    print(str(address) + ':' + data)
    action = js['action']
    respond = ''
    if action == 'tongbu':
        tongbuB.receive_tongbu_message(js, conn, address)
    elif action == 'send':
        tongbuB.add_chatlog(js)
        tongbuB.new_chatlog(js)
        respond = json.dumps({'state': True})
    elif action == 'read':
        data = tongbuB.remove_chatlog(js['to'])
        if len(data) > 0:
            tongbuB.old_chatlog(js['to'])
        respond = json.dumps(data)
    if respond != '':
        conn.send(respond.encode('UTF-8'))
    conn.close()


if __name__ == '__main__':
    main()

