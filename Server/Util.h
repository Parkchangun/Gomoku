#ifndef GOMOKU_UTIL_H
	#define GOMOKU_UTIL_H
using namespace std;
#include<vector>
#include<sstream>
class Util {
	public:
		//�Է� ���� ���ڿ��� delimiter�� �������� ��ū���� ����
		vector<string> getTokens(string input, char delimiter);
	};
#endif // !GOMOKU_UTIL_H
