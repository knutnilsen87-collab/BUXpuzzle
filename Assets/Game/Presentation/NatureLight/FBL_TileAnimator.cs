using UnityEngine;
using System.Collections;

public class FBL_TileAnimator : MonoBehaviour
{
Transform t;

void Awake()
{
t=transform;
}

public void Select()
{
StopAllCoroutines();
StartCoroutine(ScaleTo(1.08f,0.09f));
}

public void Deselect()
{
StopAllCoroutines();
StartCoroutine(ScaleTo(1f,0.12f));
}

public void MatchPop()
{
StopAllCoroutines();
StartCoroutine(Pop());
}

IEnumerator Pop()
{
yield return ScaleTo(0.94f,0.06f);
yield return ScaleTo(1.06f,0.06f);
yield return ScaleTo(0f,0.14f);
}

public void Fall(Vector3 target,float duration)
{
StartCoroutine(FallRoutine(target,duration));
}

IEnumerator FallRoutine(Vector3 target,float duration)
{
Vector3 start=t.position;

float time=0;

while(time<duration)
{
time+=Time.deltaTime;

float p=time/duration;

t.position=Vector3.Lerp(start,target,p);

yield return null;
}

t.position=target;

yield return ScaleTo(1.05f,0.04f);
yield return ScaleTo(1f,0.08f);
}

IEnumerator ScaleTo(float s,float d)
{
Vector3 start=t.localScale;
Vector3 end=Vector3.one*s;

float time=0;

while(time<d)
{
time+=Time.deltaTime;

float p=time/d;

t.localScale=Vector3.Lerp(start,end,p);

yield return null;
}

t.localScale=end;
}
}
