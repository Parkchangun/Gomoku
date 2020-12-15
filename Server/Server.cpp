#include <Server.h>

SOCKET Server::serversocket;
WSADATA Server::wsaData;
SOCKADDR_IN Server::serverAddress;
int Server::nextID;
vector<Client*> Server::connections;
Util server::util;

void Server::enterClient(Client* client) {
	char* sent = new char[256];
	ZeroMemory(sent, 256);
	sprintf(sent, "%s", "[Enter]");
	send(client->getClientSocket(), sent, 256, 0);
}

void Server::fullClient(Client* client) {
	char* sent = new char[256];
	ZeroMemory(sent, 256);
	sprintf(sent, "%s", "[Full]");
	send(client->getClientSocket(), sent, 256, 0);
}

void Server::playClient(int roomID) {
	char* sent = new char[256];
	bool black = true;

	for (int i = 0; i < connections.size(); i++) {
		if (connections[i]->getRoomID == roomID) {
			ZeroMemory(sent, 256);
			if (black) {
				sprintf(sent, "%s", "[Play]Black");
				black = false;
			}
			else {
				sprintf(sent, "%s", "[Play]White");
			}
			send(connections[i]->getClientSocket(), sent, 256, 0);
		}
	}
}

void Server::exitClient(int roomID) {
	char* sent = new char[256];
	for (int i = 0; i < connections.size(); i++) {
		if (connections[i]->getRoomID == roomID) {
			ZeroMemory(sent, 256);
			sprintf(sent, "%s", "[Exit]");
			send(connections[i]->getClientSocket(), sent, 256, 0);
		}
	}
}
//�ٵϵ��� ���� ��ġ�� �������ִ� �Լ�
void Server::putClient(int roomID, int x, int y) {
	char* sent = new char[256];
	for (int i = 0; i < connections.size(); i++) {
		if (connections[i]->getRoomID == roomID) {
			ZeroMemory(sent, 256);
			string data = "[Put]" + to_string(x) + "," + to_string(y);
			sprintf(sent, "%s", data.c_str());
			send(connections[i]->getClientSocket(), sent, 256, 0);
		}
	}
}
//���������� Ư���� ���� �ܷ��ο��� Ȯ�����ִ� �Լ�
int Server::clientCountInRoom(int roomID) {
	int count = 0;
	for (int i = 0; i < connections.size(); i++) {
		if (connections[i]->getRoomID() == roomID) {
			count++;
		}
	}
	return count;
}

void Server::start() {
	WSAStartup(MAKEWORD(2, 2), &wsaData);
	serverSocket = socket(AF_INET, SOCK_STREAM, NULL);

	serverAddress.sin_addr.s_addr = inet_addr("127.0.0.1");
	serverAddress.sin_port = htons(9876);
	serverAddress.sin_family = AF_INET;

	cout << "[ C++ ���� ���� ���� ON ]" << endl;

	bind(serverSocket, (SOCKADDR*)&serverAddress, sizeof(serverAddress));
	listen(serverSocket, 32);

	int addressLength = sizeof(serverAddress);

	while (true) {
		SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, NULL);
		if (clientSocket = accept(serverSocket, (SOCKADDR*)&serverAddress, &addressLength)) {
			Client* client = new Client(nextID, clientSocket);
			cout << "[ New User Connect ]" << endl;

			CreateThread(NULL, NULL, (LPTHREAD_START_ROUTINE)ServerThread, (LPVOID)client, NULL, NULL);
			connections.push_back(*client);
			nextID++;
		}
		Sleep(100);
	}
}

void ServerThread(Client* client) {
	char* sent = new char[256];
	char* received = new char[256];
	int size = 0;

	while (true) {
		ZeroMemory(received, 256);
		if ((size = recv(client->getClientSocket(), received, 256, NULL)) > 0) {
			string receivedString = string(received);
			vector<string> tokens = getTokens(receivedString, ']');

			if (receivedString.find("[Enter]") != -1) {
				/* �޽����� ���� Ŭ���̾�Ʈ ã�� */
				for (int i = 0; i < connections.size(); i++) {
					string roomID = tokens[1];
					int roomInt = atoi(roomID.c_str());
					//Client�� ã�� ���
					if (connections[i].getClientSocket() == client->getClientSocket()) {
						int countClient = countClientInRoom(roomInt);
						//2�� �̻��� ������ �濡 �� �ִ� ��� ���� á�ٰ� ����
						if (countClient >= 2) {
							ZeroMemory(sent, 256);
							sprintf(sent, "%s", "[Full]");
							send(connections[i].getClientSocket(), sent, 256, 0);
							break;
						}
						//2�� �̸��� ���
						cout << "Ŭ���̾�Ʈ [" << client->getClientID() << "]: " << roomID << "�� ������ ����" << endl;
						//�ش� ������� �� ���� ���� ����
						Client* newClient = new Client(*client);
						newClient->setRoomID(roomInt);
						connections[i] = *newClient;
						//�濡 �����Ͽ��ٰ� �޽��� ����
						ZeroMemory(sent, 256);
						sprintf(sent, "%s", "[Enter]");
						send(connections[i].getClientSocket(), sent, 256, 0);
						//������ ���ӵǾ��ִ� ��� ���� ����
						if (countClient == 1) {
							playClient(roomInt);
						}
					}
				}
			}
			else if (receivedString.find("[Put]") != -1) {
				//�޽����� ���� Ŭ���̾�Ʈ ���� �ޱ�
				string data = tokens[1];
				vector<string> dataTokens = getTokens(data, ',');
				int roomID = atoi(dataTokens[0].c_str());
				int x = atoi(dataTokens[1].c_str());
				int y = atoi(dataTokens[2].c_str());
				//����ڰ� ���� ���� ��ġ�� ����
				putClient(roomID, x, y);
			}
			else if (receivedString.find("[Play]") != -1) {
				//�޽����� ���� Ŭ���̾�Ʈ ã��
				string roomID = tokens[1];
				int roomInt = atoi(roomID.c_str());

				playClient(roomInt);
			}
		}
		else {
			cout << "Ŭ���̾�Ʈ [" << client->getClientID() << "]�� ������ ������." >> endl;

			//���ӿ��� ���� �÷��̾� ã��
			for (int i = 0; i < connections.size(); i++) {
				if (connections[i].getClientID() == client->getClientID()) {
					//�ٸ� ����ڿ� ���� ���̴� ����� ���� ���
					if (connections[i].getRoomID() != -1 &&
							countClientInRoom(connections[i].getRoomID()) == 2) {
						exitClient(connections[i].getRoomID());
					}
					connections.erase(connections.begin() + i);
					break;
				}
			}
			delete client;
			break;
		}
	}
}