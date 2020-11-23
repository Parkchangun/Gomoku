#ifndef GOMOKU_CLIENT_H
#define GOMOKU_CLIENT_H
#include <winsock.h>
class Client
{
public:
	Client(int clientID, SOCKET clientSocket);
	int getClientID();
	int getRoomID();
	void setRoomID(int roomID);
	SOCKET getClientSocket();


private:
	int clientID;
	int roomID;
	SOCKET clientSocket;
};

#endif // !GOMOKU_CLIENT_H

