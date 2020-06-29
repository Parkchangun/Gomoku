#define _CRT_SECURE_NO_WARNINGS
#include <Windows.h>
#include <winsock.h>
#include <iostream>
#include <vector>
#include <sstream>

using namespace std;
//Client Ŭ���� ����
class Client {
private:
	int clientID;
	int roomID;
	SOCKET clientSocket;
public:
	Client(int clientID, SOCKET clientSocket) {
		this->clientID = clientID;
		this->roomID = -1;
		this->clientSocket = clientSocket;
	}

	int getClientID() {
		return clientID;
	}

	int getRoomID() {
		return roomID;
	}

	void setRoomID(int roomID) {
		this->roomID = roomID;
	}

	SOCKET getClientSocket() {
		return clientSocket;
	}
};

SOCKET serverSocket;
vector<Client> connections;
WSADATA wsaData;
SOCKADDR_IN serverAddress;

int nextID;

vector<string> getTokens(string input, char delimiter) {
	vector<string> tokens;
	istringstream format(input);
	string str;

	while (getline(format, str, delimiter)) {
		tokens.push_back(str);
	}

	return tokens;
}

int countClientInRoom(int roomID) {
	int count = 0;

	for (int i = 0; connections.size(); i++) {
		if (connections[i].getRoomID() == roomID) {
			count++;
		}
	}

	return count;
}

void playClient(int roomID) {
	char* sent = new char[256];
	bool black = true;

	for (int i = 0; i < connections.size(); i++) {
		if (connections[i].getRoomID() == roomID) {
			ZeroMemory(sent, 256);
			
			if (black) {
				sprintf(sent, "%s", "[Play]Black");
				black = false;
			}
			else {
				sprintf(sent, "%s", "[Play]White");
			}
			send(connections[i].getClientSocket(), sent, 256, 0);
		}
	}
}

void exitClient(int roomID) {
	char* sent = new char[256];

	for (int i = 0; i < connections.size(); i++) {
		if (connections[i].getRoomID() == roomID) {
			ZeroMemory(sent, 256);
			sprintf(sent, "%s", "[Exit]");
			send(connections[i].getClientSocket(), sent, 256, 0);
		}
	}
}

void putClient(int roomID, int x, int y) {
	char* sent = new char[256];
	for (int i = 0; i < connections.size(); i++) {
		if (connections[i].getRoomID() == roomID) {
			ZeroMemory(sent, 256);
			string data = "[Put]" + to_string(x) + ", " + to_string(y);
			sprintf(sent, "%s", data.c_str());
			send(connections[i].getClientSocket(), sent, 256, 0);
		}
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
		}
	}
}

int main() {
	WSAStartup(MAKEWORD(2, 2), &wsaData);
	serverSocket = socket(PF_INET, SOCK_STREAM, NULL);

	serverAddress.sin_addr.S_un.S_addr = inet_addr("127.0.0.1");
	serverAddress.sin_port = htons(9876);
	serverAddress.sin_family = AF_INET;

	cout << "[ C++ ���� ���� ���� ON ]" << endl;

	bind(serverSocket, (SOCKADDR*)&serverAddress, sizeof(serverAddress));
	listen(serverSocket, 4);

	int addressLength = sizeof(serverAddress);

	while (true) {
		SOCKET clientSocket = socket(PF_INET, SOCK_STREAM, NULL);
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