using System;

namespace UniversalModule.DelaySystem {
    public enum NodeType {
        Invalid,
        NoParameters,
        HasParameters,
    }

    public class DelayNodeBase {
        /// <summary>
        /// �¼�������
        /// </summary>
        internal object sender;
        /// <summary>
        /// �ӳ�ʱ��
        /// </summary>
        internal float delayTime;
        /// <summary>
        /// ע��ʱ��
        /// </summary>
        internal float registerTime;

        /// <summary>
        /// �ڵ�����
        /// </summary>
        internal virtual NodeType NodeType { get; }

        /// <summary>
        /// �ڵ�����¼�
        /// </summary>
        internal static Action<DelayNodeBase> RecycleNodeListener;

        /// <summary>
        /// ���ûص��¼�
        /// </summary>
        internal virtual void Invoke() { throw new Exception("This interface has not yet been implemented."); }
        /// <summary>
        /// ���ýڵ����в���
        /// </summary>
        internal virtual void Reset() {
            delayTime = 0;
            sender = null;
            registerTime = 0;
        }
        /// <summary>
        /// ���ոýڵ�
        /// </summary>
        /// <exception cref="Exception"></exception>
        internal void Recycle() {
            if (RecycleNodeListener == null) {
                throw new Exception("Recycle event has not been registered.");
            }
            if (NodeType == NodeType.Invalid) {
                throw new Exception("Invalid delay node!");
            }
            RecycleNodeListener(this);
        }
    }
}
