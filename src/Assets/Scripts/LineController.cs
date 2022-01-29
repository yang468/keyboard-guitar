using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonicBloom.Koreo;
public class LineController : MonoBehaviour
{
    RhythmGameController gameController;
    [Tooltip("������ʹ�õļ��̰���")]
    public KeyCode keyboardButton;

    public Transform targetVisuals;
    public Transform targetTopTrans;
    public Transform targetBottomTrans;

    [Tooltip("�������Ӧ�¼��ı��")]
    public int laneID;
    List<KoreographyEvent> laneEvents = new List<KoreographyEvent>();
    //���������쵱ǰ���������������Ķ��У��ȳ��ֵ���������ʧ
    Queue<NoteObject> trackedNotes = new Queue<NoteObject>();
    //���������е����ɵ���һ���¼�������
    int pendingEventIdx = 0;
    //���µ���Ч
    public GameObject downVisual;
    //�����ƶ���Ŀ��λ��
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
        //�����Ч����
        while (trackedNotes.Count > 0&&trackedNotes.Peek().isNoteMissed())
        {
            if (trackedNotes.Peek().isLongNoteEnd)
            {
                hasLongNote = false;
                timeVal = 0;
            }
            trackedNotes.Dequeue();
        }
        //����������Ĳ���
        CheckSpawnNext();
        //�����ҵ�����
        if (Input.GetKeyDown(keyboardButton))
        {
            CheckNoteHit();
            downVisual.SetActive(true);//��Ч����
        }
        else if (Input.GetKey(keyboardButton))
        {
            //��ⳤ����
            if (hasLongNote)
            {
                if (timeVal>=0.15f)
                {
                    //��ʾ���еȼ�
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
            downVisual.SetActive(false);//��Ч����
            //��ⳤ����
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
    //���ƥ�䣬��ѵ�ǰ�¼���ӽ��¼��б�
    public void AddEventToLane(KoreographyEvent evt)
    {
        laneEvents.Add(evt);
    }
    //�����������ϲ�����λ�õ�ƫ����
    int GetSpawnSampleOffset()
    {
        //����λ�ú�Ŀ����λ��
        float spawnDistToTarget = targetTopTrans.position.z - transform.position.z;
        //����Ŀ����ʱ��
        float spawnPosToTargetTime = spawnDistToTarget / gameController.noteSpeed;
        return (int)spawnPosToTargetTime*gameController.SampleRate;
    }
    //����Ƿ�������һ��������
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
            //�ж��Ƿ�Ϊ������
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
    //���ɰ�����Ч�ķ���
    void CreateDownEffect()
    {
        //Ҫ�ж���ȥ������Ч
        GameObject downEffectGo = gameController.GetFreshEffectObject(gameController.downEffectObjectPool,gameController.downEffectGo);
        downEffectGo.transform.position = targetVisuals.position;
    }
    //���е���Ч
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
    //����Ƿ��л�����������
    public void CheckNoteHit()
    {
        if (trackedNotes.Count>0)
        {
            NoteObject noteObject = trackedNotes.Peek();
            if (noteObject.hitOffset>-6000)
            {
                trackedNotes.Dequeue();//����
                int hitLevel = noteObject.IsNoteHittable();
                if (hitLevel > 0)
                {
                    //���д���
                    //���·���
                    gameController.UpdateScoreText(100 * hitLevel);
                    //����������Ч
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
                    //δ����

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
