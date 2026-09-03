using Core.DataModels;

public interface IHitFeedbackReceiver
{
    void ReceiveHitFeedback(HitFeedback feedback);
}