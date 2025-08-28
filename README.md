# Playing-Card-Game-Server

* 이 README는 Google Gemini의 도움을 받아 작성되었습니다.

---

## Blackjack 서버 (Blackjack Server)

## 📖 개요 (Overview)

이 프로젝트는 ASP.NET Core와 SignalR을 사용하여 구축된 실시간 멀티플레이어 블랙잭 게임 서버입니다.
[Playing-Card-Game](https://github.com/pjb24/Playing-Card-Game) 클라이언트 프로젝트와 함께 사용하도록 설계되었습니다.
여러 플레이어가 함께 게임 룸에 참여하여 베팅하고 블랙잭 게임을 즐길 수 있는 게임 시스템을 구성합니다.

## ✨ 주요 기능 (Features)

* 실시간 통신을 위한 SignalR 기반 웹소켓 연결
* 게임 룸 생성, 참여 및 목록 조회
* 플레이어 간 베팅 기능
* 블랙잭 기본 규칙 구현 (Hit, Stand, Double Down, Split 등)
* 상태 패턴(State Pattern)을 이용한 게임 흐름 관리

## 🛠️ 기술 스택 (Tech Stack)

* **Backend:** C#, ASP.NET Core 9.0
* **Real-time Communication:** SignalR
* **IDE:** Visual Studio / VS Code

## 🚀 시작하기 (Getting Started)

이 서버는 WSL(Windows Subsystem for Linux)의 Ubuntu 환경에서 개발 및 테스트되었습니다.

### 설치 및 실행 (Installation & Running)

이 프로젝트는 서버 애플리케이션입니다. 게임을 플레이하려면 클라이언트 프로젝트를 함께 실행해야 합니다.

1. **서버 저장소 복제 (Clone the Server repository):**

    ``` bash
    git clone https://github.com/pjb24/Playing-Card-Game-Server.git
    cd Playing-Card-Game-Server/BlackjackServer
    ```

2. **서버 의존성 복원 및 실행 (Run the Server):**

    ```bash
    dotnet run
    ```

3. **클라이언트 저장소 복제 및 실행 (Clone & Run the Client):**
    *별도의 터미널에서 진행*

    ```bash
    git clone https://github.com/pjb24/Playing-Card-Game.git
    # Unity Editor에서 프로젝트를 열고 실행합니다.
    ```

4. 서버가 시작되면 `https://localhost:5065` (또는 `launchSettings.json`에 지정된 다른 포트)에서 실행됩니다. 클라이언트는 이 주소로 서버에 연결합니다.

## 🎮 사용 방법 (How to Use / API)

클라이언트는 SignalR을 통해 서버의 `BlackjackHub`에 연결하여 통신합니다.

* **Hub URL:** `/blackjackHub`

* **주요 클라이언트 호출 메서드 (Client -> Server):**
* `CreateNewRoom`: 새 게임 룸을 생성합니다.
* `JoinLobby`: 로비에 참여하여 룸 목록을 받습니다.
* `JoinRoom`: 특정 룸에 참여합니다.
* `PlaceBet`: 게임 시작 전 베팅합니다.
* `Hit`, `Stand`, `DoubleDown`, `Split`: 블랙잭 액션을 수행합니다.
* `StartGame`: 룸의 모든 플레이어가 준비되면 게임을 시작합니다.

## 📁 프로젝트 구조 (Project Structure)

```plaintext
/
├── Game/             # 게임의 핵심 로직, 도메인, 상태 머신 포함
│   ├── Domain/       # 카드, 덱, 플레이어 등 핵심 객체
│   ├── FSM/          # 게임 상태(베팅, 딜링 등) 관리
│   └── Network/      # DTO 및 커맨드 핸들러
├── Hubs/             # SignalR 허브
├── Services/         # 게임 룸 및 유저 관리 서비스
├── Properties/       # 프로젝트 실행 설정
└── wwwroot/          # 정적 파일 (테스트용 HTML 등)
```
