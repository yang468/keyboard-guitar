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
        //判断音符是否处于miss状态
        if (transform.position.z<=lineController.targetBottomTrans.position.z)
        {
            gameController.ReturnNoteObjectToPool(this);
            ResetNote();
        }
    }
    //初始化方法
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
    //将note对象重置
    private void ResetNote()
    {
        trackedEvent = null;
        lineController = null;
        gameController = null;
    }
    //返回对象池
    void ReturnToPool()
    {
        gameController.ReturnNoteObjectToPool(this);
        ResetNote();

    }
    //击中音符
    public void OnHit()
    {
        ReturnToPool();
    }
    //更新位置的方法
    void UpdatePosition()
    {
        Vector3 pos = lineController.TargetPosition;
        pos.z -= (gameController.DelayedSampleTime - trackedEvent.StartSample)/(float)gameController.SampleRate*gameController.noteSpeed;
        transform.position = pos;
    }
    void GetHitOffset()
    {
        int curTime = gameController.DelayedSampleTime;
        int noteTime = trackedEvent.StartSample;//事件所在时间
        int hitWindow = gameController.HitWindowSampleWidth;//命中窗口
        hitOffset = hitWindow - Mathf.Abs(noteTime - curTime);//取notetime和curTime的差值绝对值与hitWindow比较
    }
    //当前音符是否已经miss
    public bool isNoteMissed()
    {
        bool bMissed = true;
        if (enabled)
        {
            int curTime = gameController.DelayedSampleTime;
            int noteTime = trackedEvent.StartSample;//事件所在时间
            int hitWindow = gameController.HitWindowSampleWidth;//命中窗口
            bMissed = curTime - noteTime > hitWindow;
        }
        return bMissed;
    }
    public int IsNoteHittable()
    {
        int hitLevel = 0;//击中等级，0位miss，1为great，2为perfect
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
