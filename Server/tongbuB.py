import socket
import json
import serverB
import threading
import time
import random

peers = []
chatlog = {}
#test_chatlog = {'chatlog_update_times': 0, 'zhangtuo': }
chatlog_update_times = 0
user_last_check_time = {}
heart_beat_time = 5
last_peer_tongbu_time = 0
last_chatlog_tongbu_time = 0


def init(tongbu_server, tongbu_port):
    add_peer(serverB.my_ip, serverB.my_port, serverB.my_type)
    threading.Thread(target=heart_beat, args=(heart_beat_time, )).start()
    tongbu(tongbu_server, tongbu_port)


def tongbu(tongbu_server, tongbu_port):
    global chatlog
    if tongbu_server == '' or tongbu_port == 0:
        chatlog = {'chatlog_update_times': 0}
        return
    add_peer(tongbu_server, tongbu_port, serverB.tongbu_type)
    tongbu_peer(tongbu_server, tongbu_port)
    tongbu_chatlog(tongbu_server, tongbu_port)


def tongbu_chatlog(tongbu_server, tongbu_port):
    global chatlog, last_chatlog_tongbu_time, negative_chatlog
    if time.time() - last_chatlog_tongbu_time < 3:
        print('离上次同步不到3秒，暂时不同步')
        return
    print('从 ' + str(tongbu_server) + ':' + str(tongbu_port) + ' 同步chatlog数据')
    conn = get_tongbu_connection(tongbu_server, tongbu_port)
    ask_for_chatlog(conn)
    negative_chatlog = {}
    conn.close()
    last_chatlog_tongbu_time = time.time()


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
    ask = {'action': 'tongbu', 'action2': 'ask_for_peer', 'my_ip': serverB.my_ip, 'my_port': serverB.my_port,
           'my_type': serverB.my_type}
    conn.send(json.dumps(ask).encode('UTF-8'))
    respond = conn.recv(1024).decode('UTF-8')
    if respond == '':
        peers = []
    else:
        peers = json.loads(respond)
    print('同步到peer:' + str(peers))


def ask_for_chatlog(conn):
    global chatlog
    ask = {'action': 'tongbu', 'action2': 'ask_for_chatlog'}
    conn.send(json.dumps(ask).encode('UTF-8'))
    respond = conn.recv(1024).decode('UTF-8')
    if respond == '':
        chatlog = {}
        chatlog['chatlog_update_times'] = 0
    else:
        chatlog = json.loads(respond)
    print('同步到chatlog:' + str(chatlog))


def receive_tongbu_message(js, conn, address):
    global chatlog, peers
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
    elif action == 'ask_for_chatlog':
        print('收到来自' + str(address) + '的同步chatlog的请求')
        respond = json.dumps(chatlog)
    elif action == 'add_peer':
        print('收到来自' + str(address) + '的添加peer的请求')
        ip = js['ip']
        port = js['port']
        add_peer(ip, port, js['type'])
    elif action == 'add_chatlog':
        print('收到来自' + str(address) + '的添加chatlog的请求')
        add_chatlog(js)
        respond = json.dumps({'state': True})
    elif action == 'check':
        print('收到来自' + str(address) + '的心跳')
        if js.get('chatlog_num') is not None:
            check_chatlog(js['chatlog_num'])
        if js.get('type') is not None:
            add_peer(js['from_ip'], js['from_port'], js['type'])
        check_peer(js['peer_num'])
    elif action == 'remove_chatlog':
        until_time = js['until_time']
        user = js['to']
        remove_charlog_until(user, until_time)
    if respond != '':
        conn.send(respond.encode('UTF-8'))


def new_peer(ip, port, type):
    print('广播新的peer ' + str(ip) + ':' + str(port))
    message = {'action': 'tongbu', 'action2': 'add_peer', 'ip': ip, 'port': port, 'type': type}
    broadcast_to_peers(json.dumps(message), type='ALL')


def new_chatlog(js):
    print('广播新的chatlog' + js['message'])
    message = js
    message['action'] = 'tongbu'
    message['action2'] = 'add_chatlog'
    broadcast_to_peers(json.dumps(message), type='B')


def old_chatlog(user):
    last_time = user_last_check_time[user]
    message = {'action': 'tongbu', 'action2': 'remove_chatlog', 'to': 'zhangtuo', 'until_time': last_time}
    broadcast_to_peers(json.dumps(message), type='B')


def add_chatlog(js):
    global chatlog, chatlog_update_times
    print('添加chatlog ' + str(js))
    tos = js['to']
    room = tos + [js['from']]
    for to in tos:
        if chatlog.get(to) is None:
            chatlog[to] = []
        t = js['time']
        if user_last_check_time.get(to) is None or t > user_last_check_time[to]:
            chatlog[to].append({'from': js['from'], 'message': js['message'], 'time': js['time'], 'room': room})
    chatlog_update_times += 1
    chatlog['chatlog_update_times'] += 1


def remove_chatlog(user):
    global chatlog_update_times, chatlog
    if chatlog.get(user) is None:
        return []
    logs = chatlog[user]
    if len(logs) == 0:
        return logs
    last_time = logs[-1]['time']
    user_last_check_time[user] = last_time
    chatlog_update_times += 1
    chatlog['chatlog_update_times'] += 1
    chatlog[user] = []
    return logs


def remove_charlog_until(user, until_time):
    global chatlog, chatlog_update_times
    if chatlog.get(user) is None:
        return
    logs = chatlog[user]
    logs = [log for log in logs if log['time'] > until_time]
    chatlog[user] = logs
    chatlog_update_times += 1
    chatlog['chatlog_update_times'] += 1


def add_peer(ip, port, type):
    global peers
    for peer in peers:
        if peer['ip'] == ip and peer['port'] == port:
            peer['alive'] = True
            return
    print('添加peer ' + str(ip) + ':' + str(port))
    peers.append({'ip': ip, 'port': port, 'alive': True, 'type': type})


def check_chatlog(num):
    if chatlog['chatlog_update_times'] < num:
        print('检测到chatlog不同步')
        peer = random_choice('B')
        if peer is not None:
            tongbu_chatlog(peer['ip'], peer['port'])


def check_peer(num):
    if len(peers) < num:
        print('检测到peer不同步')
        peer = random_choice('ALL')
        if peer is not None:
            tongbu_peer(peer['ip'], peer['port'])


def heart_beat(sleep_sec):
    global chatlog_update_times
    while True:
        time.sleep(sleep_sec)
        message = {'action': 'tongbu', 'action2': 'check', 'peer_num': len(peers), 'chatlog_num': chatlog_update_times,
                   'type': serverB.my_type, 'from_ip': serverB.my_ip, 'from_port': serverB.my_port}
        print('心跳')
        print('当前的peers：' + str(peers))
        print('当前的chatlogs：' + str(chatlog))
        broadcast_to_peers(json.dumps(message), type='ALL')


def broadcast_to_peers(message, type='B'):
    print('广播消息: ' + str(message))
    for peer in peers:
        if peer['type'] != type and type != 'ALL':
            continue
        if not peer['alive']:
            continue
        if peer['ip'] == serverB.my_ip and peer['port'] == serverB.my_port:
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
    available = [p for p in peers if (p['ip'] != serverB.my_ip or p['port'] != serverB.my_port)
                 and (p['type'] == type or type == 'ALL') and p['alive']]
    if len(available) == 0:
        return None
    return random.choice(available)

