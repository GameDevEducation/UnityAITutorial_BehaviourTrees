public interface IPausable
{
    bool OnPauseRequested();

    bool OnResumeRequested();

    void OnPause();

    void OnResume();
}