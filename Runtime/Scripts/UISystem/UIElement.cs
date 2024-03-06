using BrunoMikoski.AnimationSequencer;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace SeroJob.UiSystem
{
    public class UIElement : MonoBehaviour
    {
        [SerializeField] private UIElementOpenBehaviour _openBehaviour = UIElementOpenBehaviour.FixedToPage;
        [SerializeField] private UIElementCloseBehaviour _closeBehaviour = UIElementCloseBehaviour.FixedToPage;

        #region Animations

        [SerializeField] [Foldout("Animations")] protected AnimationSequencerController openAnim;
        [SerializeField] [Foldout("Animations")] protected AnimationSequencerController closeAnim;

        #endregion

        #region Events 

        [Foldout("Events")] public UnityEvent<UIElement> OnOpened;
        [Foldout("Events")] public UnityEvent<UIElement> OnClosed;

        #endregion

        public UIElementOpenBehaviour OpenBehaviour => _openBehaviour;
        public UIElementCloseBehaviour CloseBehaviour => _closeBehaviour;

        #region Public Methods

        public virtual void Open()
        {
            openAnim.Play(() => OnOpened?.Invoke(this));
        }

        public virtual void Close()
        {
            closeAnim.Play(() => OnClosed?.Invoke(this));
        }

        public virtual void OpenImmediately()
        {
            openAnim.Play();
            openAnim.Complete(true);

            OnOpened?.Invoke(this);
        }

        public virtual void HideImmediately()
        {
            closeAnim.Play();
            closeAnim.Complete(true);

            OnClosed?.Invoke(this);
        }

        public virtual void PageStartedOpening()
        {
            switch (_openBehaviour)
            {
                case UIElementOpenBehaviour.FixedToPage:

                    OpenImmediately();
                    OnOpened?.Invoke(this);

                    break;

                case UIElementOpenBehaviour.AnimateWithPage:

                    HideImmediately();
                    openAnim.Play();
                    OnOpened?.Invoke(this);

                    break;

                case UIElementOpenBehaviour.AnimateAfterPage:

                    HideImmediately();

                    break;

                case UIElementOpenBehaviour.Manuel:

                    HideImmediately();

                    break;
            }
        }

        public virtual void PageFinishedOpening()
        {
            switch (_openBehaviour)
            {
                case UIElementOpenBehaviour.AnimateAfterPage:

                    openAnim.Play(() =>
                    {
                        OnOpened?.Invoke(this);
                    });

                    break;
            }
        }

        public virtual void PageStartedClosing()
        {
            if (_closeBehaviour == UIElementCloseBehaviour.FixedToPage)
            {
                OnClosed?.Invoke(this);
                return;
            }

            if (_closeBehaviour == UIElementCloseBehaviour.AnimateWithPage)
            {
                closeAnim.Play();
                OnClosed?.Invoke(this);
                return;
            }

            if (_closeBehaviour == UIElementCloseBehaviour.AnimateBeforePage)
            {
                closeAnim.Play(() =>
                {
                    OnClosed?.Invoke(this);
                });
                return;
            }
        }

        public virtual void PageFinishedClosing()
        {

        }

        #endregion
    }
}