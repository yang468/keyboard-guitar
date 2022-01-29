using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonicBloom.Koreo;
public class LineController : MonoBehaviour
{
    RhythmGameController gameController;
    [Tooltip("此音轨使用的键盘按键")]
    public KeyCode keyboardButton;

    public Transform targetVisuals;
    public Transform targetTopTrans;
    public Transform targetBottomTrans;

    [Tooltip("此音轨对应事件的编号")]
    public int laneID;
    List<KoreographyEvent> laneEvents = new List<KoreographyEvent>();
    //包含此音轨当前活动的所有音符对象的队列，先出现的音符先消失
    Queue<NoteObject> trackedNotes = new Queue<NoteObject>();
    //检测此音轨中的生成的下一个事件的索引
    int pendingEventIdx = 0;
    //按下的特效
    public GameObject downVisual;
    //音符移动的目标位置
    public Vector3 TargetPosition
    {
        get
        {
            return transform.position;
        }
    }
    public bool hasLongNote;
    public float timeVal = 0;
    public GameObject longNoteHitEffectGo;
    // Update is called once per frame
    void Update()
    {
        if (gameController.isPauseState)
        {
            return;
        }
        //清除无效音符
        while (trackedNotes.Count > 0&&trackedNotes.Peek().isNoteMissed())
        {
            if (trackedNotes.Peek().isLongNoteEnd)
            {
                hasLongNote = false;
                timeVal = 0;
            }
            trackedNotes.Dequeue();
        }
        //检测新音符的产生
        CheckSpawnNext();
        //检测玩家的输入
        if (Input.GetKeyDown(keyboardButton))
        {
            CheckNoteHit();
            downVisual.SetActive(true);//特效开启
        }
        else if (Input.GetKey(keyboardButton))
        {
            //检测长音符
            if (hasLongNote)
            {
                if (timeVal>=0.15f)
                {
                    //显示命中等级
                    if (!longNoteHitEffectGo.activeSelf)
                    {
                        CreateHitLongEffect();
                    }
                    timeVal = 0;
                }
                else
                {
                    timeVal += Time.deltaTime;
                }
            }
        }
        else if (Input.GetKeyUp(keyboardButton))
        {
            downVisual.SetActive(false);//特效隐藏
            //检测长音符
            if (hasLongNote)
            {
                longNoteHitEffectGo.SetActive(false);
                CheckNoteHit();
            }
        }
    }
    public void Initialize(RhythmGameController controller)
    {
        gameController = controller;
    }
    public bool DoesMatch(int noteID)
    {
        return noteID == laneID;
    }
    //如果匹配，则把当前事件添加进事件列表
    public void AddEventToLane(KoreographyEvent evt)
    {
        laneEvents.Add(evt);
    }
    //音符在音谱上产生的位置的偏移量
    int GetSpawnSampleOffset()
    {
        //出生位置和目标点的位置
        float spawnDistToTarget = targetTopTrans.position.z - transform.position.z;
        //到达目标点的时间
        float spawnPosToTargetTime = spawnDistToTarget / gameController.noteSpeed;
        return (int)spawnPosToTargetTime*gameController.SampleRate;
    }
    //检测是否生成下一个新音符
    void CheckSpawnNext()
    {
        int samplesToTarget = GetSpawnSampleOffset();
        int currentTime = gameController.DelayedSampleTime;
        while (pendingEventIdx<laneEvents.Count&&laneEvents[pendingEventIdx].StartSample<currentTime+samplesToTarget)
        {
            KoreographyEvent evt = laneEvents[pendingEventIdx];
            int noteNum = evt.GetIntValue();
            NoteObject newObj = gameController.GetFreshNoteObject();
            bool isLongNoteStart = false;
            bool isLongNoteEnd = false;
            //判断是否为长音符
            if (noteNum > 6)
            {
                isLongNoteStart = true;
                noteNum -= 6;
                if (noteNum > 6)
                {
                    isLongNoteEnd = true;
                    isLongNoteStart = false;
                    noteNum = noteNum - 6;
                }
            }
            newObj.Initialize(evt,noteNum,this,gameController,isLongNoteStart,isLongNoteEnd);
            trackedNotes.Enqueue(newObj);
            pendingEventIdx++;
        }
    }
    //生成按下特效的方法
    void CreateDownEffect()
    {
        //要有对象去接受特效
        GameObject downEffectGo = gameController.GetFreshEffectObject(gameController.downEffectObjectPool,gameController.downEffectGo);
        downEffectGo.transform.position = targetVisuals.position;
    }
    //击中的特效
    void CreateHitEffect()
    {
        GameObject hitEffectGo = gameController.GetFreshEffectObject(gameController.hitEffectObjectPool, gameController.hitEffectGo);
        hitEffectGo.transform.position = targetVisuals.position;
    }
    void CreateHitLongEffect()
    {
        longNoteHitEffectGo.SetActive(true);
        longNoteHitEffectGo.transform.position = targetVisuals.position;
    }
    //检测是否有击中音符对象
    public void CheckNoteHit()
    {
        if (trackedNotes.Count>0)
        {
            NoteObject noteObject = trackedNotes.Peek();
            if (noteObject.hitOffset>-6000)
            {
                trackedNotes.Dequeue();//出队
                int hitLevel = noteObject.IsNoteHittable();
                if (hitLevel > 0)
                {
                    //击中处理
                    //更新分数
                    gameController.UpdateScoreText(100 * hitLevel);
                    //产生击中特效
                    if (noteObject.isLongNote)
                    {
                        hasLongNote = true;
                        CreateHitLongEffect();
                    }
                    else if (noteObject.isLongNoteEnd)
                    {
                        hasLongNote = false;
                    }
                    else
                    {
                        CreateHitEffect();
                    }
                }
                else
                {
                    //未击中

                }
                noteObject.OnHit();
            }
            else
            {
                CreateDownEffect();
            }
        }
    }
}
