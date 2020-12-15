#ifndef GOMOKU_UTIL_H
	#define GOMOKU_UTIL_H
using namespace std;
#include<vector>
#include<sstream>
class Util {
	public:
		//입력 받은 문자열을 delimiter를 기준으로 토큰으로 나눔
		vector<string> getTokens(string input, char delimiter);
	};
#endif // !GOMOKU_UTIL_H
