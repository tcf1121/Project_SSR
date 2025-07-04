# Convention

## 1. Commit 메시지 구조

기본 적인 커밋 메시지 구조는 제목,본문,꼬리말 세가지 파트로 나누고, 각 파트는 빈줄을 두어 구분한다.

type : subject

body

footer

## 2. Commit Type

타입은 태그와 제목으로 구성되고, 태그는 영어로 쓰되 첫 문자는 대문자로 한다.
대괄호 안에 제목을 작성한다.

태그 : 제목의 형태이며, :뒤에만 space가 있음에 유의한다.

Feat : 새로운 기능 추가
Fix : 버그 수정
Update : 기능 업데이트
Design : 코드에는 변경이 없으나 디자인에 변경이 있을 경우
Refactor : 코드 리펙토링
Test : 테스트 코드, 리펙토링 테스트 코드 추가
Set : 프로젝트, 기타 환경 설정 등
Chore : 그 외 기타 수정
Docs : 문서 수정(주석 수정)

## 3. Subject

제목은 최대 50글자가 넘지 않도록 하고 마침표 및 특수기호는 사용하지 않는다.
한글로 간결하게 표기한다.

## 4. Body

본문은 다음의 규칙을 지킨다.

본문은 한 줄 당 72자 내로 작성한다.
본문 내용은 양에 구애받지 않고 최대한 상세히 작성한다.
본문 내용은 어떻게 변경했는지 보다 무엇을 변경했는지 또는 왜 변경했는지를 설명한다.

## 5. footer

꼬릿말은 다음의 규칙을 지킨다.

꼬리말은 optional이고 이슈 트래커 ID를 작성한다.
꼬리말은 "유형: #이슈 번호" 형식으로 사용한다.
여러 개의 이슈 번호를 적을 때는 쉼표(,)로 구분한다.
이슈 트래커 유형은 다음 중 하나를 사용한다.

- Fixes: 이슈 수정중 (아직 해결되지 않은 경우)
- Resolves: 이슈를 해결했을 때 사용
- Ref: 참고할 이슈가 있을 때 사용
- Related to: 해당 커밋에 관련된 이슈번호 (아직 해결되지 않은 경우)
  ex) Fixes: #45 Related to: #34, #23

## 6. Commit 예시

[Feat] 플레이어 움직임 구현

플레이어의 움직임을 코드로 구현하였다.

Resolves: #123
