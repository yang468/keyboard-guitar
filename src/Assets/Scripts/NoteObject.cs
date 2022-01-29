using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonicBloom.Koreo;
public class NoteObject : MonoBehaviour
{
    public SpriteRenderer visuals;
    public Sprite[] noteSprites;
    KoreographyEvent trackedEvent;
    public bool isLongNote;
    public bool isLongNoteEnd;
    LineController lineController;
    RhythmGameController gameController;

    public int hitOffset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.isPauseState)
        {
            return;
        }
        UpdatePosition();
        GetHitOffset();
        //�ж������Ƿ���miss״̬
        if (transform.position.z<=lineController.targetBottomTrans.position.z)
        {
            gameController.ReturnNoteObjectToPool(this);
            ResetNote();
        }
    }
    //��ʼ������
    public void Initialize(KoreographyEvent evt,int noteNum,LineController lineCont,RhythmGameController gameCont,bool isLongStart,bool isLongEnd)
    {
        trackedEvent = evt;
        lineController = lineCont;
        gameController = gameCont;
        isLongNote = isLongStart;
        isLongNoteEnd = isLongEnd;
        int spriteNum = noteNum;
        if (isLongNote)
        {
            spriteNum += 6;
        }
        else if (isLongEnd)
        {
            spriteNum += 12;
        }
        visuals.sprite = noteSprites[spriteNum - 1];
    }
    //��note��������
    private void ResetNote()
    {
        trackedEvent = null;
        lineController = null;
        gameController = null;
    }
    //���ض����
    void ReturnToPool()
    {
        gameController.ReturnNoteObjectToPool(this);
        ResetNote();

    }
    //��������
    public void OnHit()
    {
        ReturnToPool();
    }
    //����λ�õķ���
    void UpdatePosition()
    {
        Vector3 pos = lineController.TargetPosition;
        pos.z -= (gameController.DelayedSampleTime - trackedEvent.StartSample)/(float)gameController.SampleRate*gameController.noteSpeed;
        transform.position = pos;
    }
    void GetHitOffset()
    {
        int curTime = gameController.DelayedSampleTime;
        int noteTime = trackedEvent.StartSample;//�¼�����ʱ��
        int hitWindow = gameController.HitWindowSampleWidth;//���д���
        hitOffset = hitWindow - Mathf.Abs(noteTime - curTime);//ȡnotetime��curTime�Ĳ�ֵ����ֵ��hitWindow�Ƚ�
    }
    //��ǰ�����Ƿ��Ѿ�miss
    public bool isNoteMissed()
    {
        bool bMissed = true;
        if (enabled)
        {
            int curTime = gameController.DelayedSampleTime;
            int noteTime = trackedEvent.StartSample;//�¼�����ʱ��
            int hitWindow = gameController.HitWindowSampleWidth;//���д���
            bMissed = curTime - noteTime > hitWindow;
        }
        return bMissed;
    }
    public int IsNoteHittable()
    {
        int hitLevel = 0;//���еȼ���0λmiss��1Ϊgreat��2Ϊperfect
        if (hitOffset>0)
        {
            if (hitOffset >= 4500 && hitOffset <= 7500)
            {
                hitLevel = 2;
            }
            else hitLevel = 1;
        }
        else
        {
            this.enabled = false;
        }
        return hitLevel;
    }
}
