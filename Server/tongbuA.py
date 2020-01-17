import socket
import json
import serverA
import threading
import time
import random

peers = []
users = {}
heart_beat_time = 5
last_peer_tongbu_time = 0
last_user_tongbu_time = 0


def init(tongbu_server, tongbu_port):
    add_peer(serverA.my_ip, serverA.my_port, serverA.my_type)
    threading.Thread(target=heart_beat, args=(heart_beat_time, )).start()
    tongbu(tongbu_server, tongbu_port)


def tongbu(tongbu_server, tongbu_port):
    global users
    if tongbu_server == '' or tongbu_port == 0:
        users = {'zhangtuo': {'id': 'zhangtuo', 'password': '123'}, 'zzttwen': {'id': 'zzttwen', 'password': '123'}}# 测试用
        print('更新了测试用的user')
        return
    add_peer(tongbu_server, tongbu_port, serverA.tongbu_type)
    tongbu_peer(tongbu_server, tongbu_port)
    tongbu_user(tongbu_server, tongbu_port)


def tongbu_user(tongbu_server, tongbu_port):
    global users, last_user_tongbu_time
    if time.time() - last_user_tongbu_time < 3:
        print('离上次同步不到3秒，暂时不同步')
        return
    print('从 ' + str(tongbu_server) + ':' + str(tongbu_port) + ' 同步user数据')
    conn = get_tongbu_connection(tongbu_server, tongbu_port)
    ask_for_users(conn)
    conn.close()
    last_user_tongbu_time = time.time()


def tongbu_peer(tongbu_server, tongbu_port):
    global peers, last_peer_tongbu_time
    if time.time() - last_peer_tongbu_time < 3:
        print('离上次同步不到3秒，暂时不同步')
        return
    print('从 ' + str(tongbu_server) + ':' + str(tongbu_port) + ' 同步peer数据')
    conn = get_tongbu_connection(tongbu_server, tongbu_port)
    ask_for_peers(conn)
    conn.close()
    last_peer_tongbu_time = time.time()


def get_tongbu_connection(tongbu_server, tongbu_port):
    if tongbu_server == '' or tongbu_port == 0:
        return None
    conn = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    conn.connect((tongbu_server, tongbu_port))
    return conn


def ask_for_peers(conn):
    global peers
    ask = {'action': 'tongbu', 'action2': 'ask_for_peer', 'my_ip': serverA.my_ip, 'my_port': serverA.my_port,
           'my_type': serverA.my_type}
    conn.send(json.dumps(ask).encode('UTF-8'))
    respond = conn.recv(1024).decode('UTF-8')
    if respond == '':
        peers = []
    else:
        peers = json.loads(respond)
    print('同步到peer:' + str(peers))


def ask_for_users(conn):
    global users
    ask = {'action': 'tongbu', 'action2': 'ask_for_users'}
    conn.send(json.dumps(ask).encode('UTF-8'))
    respond = conn.recv(1024).decode('UTF-8')
    if respond == '':
        users = {}
    else:
        users = json.loads(respond)
    print('同步到user:' + str(users))


def receive_tongbu_message(js, conn, address):
    global users, peers
    action = js['action2']
    respond = ''
    if action == 'ask_for_peer':
        print('收到来自' + str(address) + '的同步peer的请求')
        respond = json.dumps(peers)
        print('发送peers' + respond)
        ip = js['my_ip']
        port = js['my_port']
        add_peer(ip, port, js['my_type'])
        new_peer(ip, port, js['my_type'])
    elif action == 'ask_for_users':
        print('收到来自' + str(address) + '的同步user的请求')
        print(users)
        respond = json.dumps(users)
    elif action == 'add_peer':
        print('收到来自' + str(address) + '的添加peer的请求')
        ip = js['ip']
        port = js['port']
        add_peer(ip, port, js['type'])
    elif action == 'add_user':
        print('收到来自' + str(address) + '的添加user的请求')
        id = js['id']
        password = js['password']
        add_user(id, password)
    elif action == 'check':
        print('收到来自' + str(address) + '的心跳')
        if js.get('user_num') is not None:
            check_user(js['user_num'])
        if js.get('type') is not None:
            add_peer(js['from_ip'], js['from_port'], js['type'])
        check_peer(js['peer_num'])
    if respond != '':
        conn.send(respond.encode('UTF-8'))


def new_peer(ip, port, type):
    print('广播新的peer ' + str(ip) + ':' + str(port))
    message = {'action': 'tongbu', 'action2': 'add_peer', 'ip': ip, 'port': port, 'type': type}
    broadcast_to_peers(json.dumps(message), type='All')


def new_user(id, password):
    print('广播新的user ' + str(id) + ':' + str(password))
    message = {'action': 'tongbu', 'action2': 'add_user', 'id': id, 'password': password}
    broadcast_to_peers(json.dumps(message), type='A')


def add_user(id, password):
    global users
    print('添加user ' + str(id) + ':' + str(password))
    users[id] = {'id': id, 'password': password}


def add_peer(ip, port, type):
    global peers
    for peer in peers:
        if peer['ip'] == ip and peer['port'] == port:
            peer['alive'] = True
            return
    print('添加peer ' + str(ip) + ':' + str(port))
    peers.append({'ip': ip, 'port': port, 'alive': True, 'type': type})


def check_user(num):
    if len(users) < num:
        print('检测到user不同步')
        peer = random_choice('A')
        if peer is not None:
            tongbu_user(peer['ip'], peer['port'])


def check_peer(num):
    if len(peers) < num:
        print('检测到peer不同步')
        peer = random_choice('ALL')
        if peer is not None:
            tongbu_peer(peer['ip'], peer['port'])


def heart_beat(sleep_sec):
    while True:
        time.sleep(sleep_sec)
        message = {'action': 'tongbu', 'action2': 'check', 'peer_num': len(peers), 'user_num': len(users),
                   'type': serverA.my_type, 'from_ip': serverA.my_ip, 'from_port': serverA.my_port}
        print('心跳')
        print('当前的peers：' + str(peers))
        print('当前的users:' + str(users))
        broadcast_to_peers(json.dumps(message), type='ALL')


def broadcast_to_peers(message, type='A'):
    print('广播消息: ' + str(message))
    for peer in peers:
        if peer['type'] != type and type != 'ALL':
            continue
        if not peer['alive']:
            continue
        if peer['ip'] == serverA.my_ip and peer['port'] == serverA.my_port:
            continue
        ip = peer['ip']
        port = peer['port']
        thread = threading.Thread(target=send_message, args=(ip, port, message))
        thread.start()


def send_message(ip, port, message):
    try:
        conn = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        conn.connect((ip, port))
        conn.send(message.encode('UTF-8'))
        conn.close()
    except ConnectionRefusedError:
        for peer in peers:
            if peer['ip'] == ip and peer['port'] == port:
                peer['alive'] = False
                break


def random_choice(type):
    available = [p for p in peers if (p['ip'] != serverA.my_ip or p['port'] != serverA.my_port)
                 and (p['type'] == type or type == 'ALL') and p['alive']]
    if len(available) == 0:
        return None
    return random.choice(available)


def send_to_random_serverB(message):
    try_times = 0
    max_try_times = 10
    respond = None
    while respond is None and try_times < max_try_times:
        respond = try_to_send_to_random_serverB(message)
        try_times += 1
    return respond


def try_to_send_to_random_serverB(message):
    try:
        server = random_choice('B')
        ip = server['ip']
        port = server['port']
        conn = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        conn.connect((ip, port))
        conn.send(message.encode('UTF-8'))
        print('向' + str(ip) + ':' + str(port) + '发送了消息:' + message)
        respond = conn.recv(1024)
        print('从' + str(ip) + ':' + str(port) + '收到了响应:' + respond.decode('UTF-8'))
        conn.close()
        return respond.decode('UTF-8')
    except ConnectionRefusedError:
        if conn is not None:
            conn.close()
        return None

