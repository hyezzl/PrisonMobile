using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 
//1. 게임을 시작하면 / 자원을 가리키는 화살표 생성

//2.자원을 5개이상캐면 / 첫번째 납부존 가리킴 + 납부존 위 화살표 활성화

//3. 납부존에 자원을 올리면 / 수갑존을 가리키는 화살표 생성 + 수갑존 위 화살표 활성화

//4. 수갑을 손에들면 / 두번째 납부존 위 화살표 활성화

//5. 수갑을 납부하면 / 머니존 위 화살표 활성화

//6. 돈을 획득하면 / (카메라 이동) + 소비존1 활성화

//7. 소비존1의 납부가 끝나면 / 소비존2 등장 + AI광부존 등장 + AI경찰존 등장

//8. 죄수가 20명 모이면 / (카메라 이동) + (감옥)소비존 등장

//9. 감옥소비존 납부끝나면 / (카메라 이동) + 감옥 넓어지는거 보여주면서 + 캐릭터에게로 카메라 돌아오며 엔딩연출
/// 
/// 
/// 
/// 
/// 
/// 
/// 
/// </summary>
public static class GameEvents
{
    // 이벤트 시작
    public struct StartEvent
    {
        public string eventID;
        public StartEvent(string eventID) { 
            this.eventID = eventID;

            Debug.Log($"{eventID} 이벤트 실행!");
        }
    }

    // 이벤트 완료
    //public struct EndEvent
    //{
    //    public string eventID;
    //    public EndEvent(string eventID) { 
    //        this.eventID = eventID;
    //    }
    //}

    // 모든 Arrow끌때
    public struct ClearArrow { }
}
