import socket
import threading
import sys
import json
import tongbuA
import time

my_ip = sys.argv[1]
my_port = int(sys.argv[2])
tongbu_server, tongbu_port, tongbu_type = '', 0, 'A'
my_type = 'A'
if len(sys.argv) > 3:
    tongbu_server, tongbu_port, tongbu_type = sys.argv[3], int(sys.argv[4]), sys.argv[5]


def main():
    tongbuA.init(tongbu_server, tongbu_port)
    start_server()


def self_as_peer():
    return {'ip': my_ip, 'port': my_port, 'type': my_type, 'alive': True}


def start_server():
    ss = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    ss.bind((my_ip, my_port))
    # ss.bind((socket.gethostname(), my_port))
    ss.listen(5)
    print('服务A启动')
    while True:
        conn, address = ss.accept()
        print('监听到消息' + str(address))
        thread = threading.Thread(target=handle_connection, args=(conn, address))
        thread.start()


def handle_connection(conn, address):
    data = str(conn.recv(1024).decode('UTF-8'))
    print(str(address) + ':' + data)
    js = json.loads(data)
    action = js['action']
    if action == 'tongbu':
        tongbuA.receive_tongbu_message(js, conn, address)
    if action == 'signin':
        signin(js, conn)
    elif action == 'login':
        login(js, conn)
    elif action == 'find_user':
        find_user(js, conn)
    elif action == 'send':
        keep_connection_on(conn, address=address, first_request=data)
    elif action == 'read':
        keep_connection_on(conn, address=address, first_request=data)
    elif action == 'logout':
        keep_connection_on(conn, address=address, first_request=data)
    conn.close()


def find_user(js, conn):
    name = js['id']
    users = [tongbuA.users[u]['id'] for u in tongbuA.users if tongbuA.users[u]['id'].startswith(name)]
    respond = {'state': True, 'result': users}
    conn.send(json.dumps(respond).encode('UTF-8'))


def signin(js, conn):
    id = js['id']
    password = js['password']
    print('注册用户:' + str(id) + '\t密码:' + str(password))
    if tongbuA.users.get(id) is not None:
        reason = '存在重名用户'
        respond = {'state': False, 'reason': reason}
    else:
        respond = {'state': True, 'reason': ''}
        tongbuA.users[id] = {'id': id, 'password': password}
        tongbuA.add_user(id, password)
        tongbuA.new_user(id, password)
    print('注册结果:' + str(respond))
    conn.send(json.dumps(respond).encode('UTF-8'))
    flush(conn)


def login(js, conn):
    print('登陆:' + str(js))
    verify_result, reason = verify_login(js)
    respond = {'state': verify_result, 'reason': reason}
    conn.send(json.dumps(respond).encode('UTF-8'))
    flush(conn)
    print('登陆结果:' + str(respond))
    if not verify_result:
        return
    #keep_connection_on(conn)


def verify_login(js):
    id = js['id']
    js_pass = js['password']
    user = tongbuA.users.get(id)
    if user is None:
        return False, '不存在该用户'
    user_pass = user['password']
    if js_pass != user_pass:
        return False, '密码错误'
    return True, ''


def keep_connection_on(conn, address = None, first_request = None):
    print('建立了一个持久连接')
    while True:
        if first_request is not None:
            message = first_request
            first_request = None
        else:
            message = str(conn.recv(1024).decode('UTF-8'))
        if address is None:
            print('收到来自持久连接的消息' + message)
        else:
            print('收到来自持久连接' + str(address) + '的消息：' + message)
        try:
            js = json.loads(message)
        except:
            continue
        action = js['action']
        respond = ''
        if action == 'send':
            if js.get('time') is None:
                js['time'] = int(time.time() * 1000)
            print('发送消息' + json.dumps(js))
            respond = tongbuA.send_to_random_serverB(json.dumps(js))
            if respond is not None:
                respond = json.dumps({'state': True})
        elif action == 'read':
            print('读取消息' + message)
            request = {'action': 'read', 'to': js['id']}
            respond = json.loads(tongbuA.send_to_random_serverB(json.dumps(request)))
            state = len(respond) >= 1
            respond = {'state': state, 'messages': respond}
            respond = json.dumps(respond)
        elif action == 'logout':
            break
        if respond != '':
            conn.send(respond.encode('UTF-8'))
            flush(conn)
            if address is not None:
                print('向客户端' + str(address) + '返回了消息:' + respond)
    if address is None:
        print('关闭了一个持久连接')
    else:
        print('关闭了持久连接' + str(address))



def flush(conn):
    conn.send('\r\n'.encode('UTF-8')) # C#用的分节符
    pass


if __name__ == '__main__':
    main()

