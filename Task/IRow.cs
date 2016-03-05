
namespace Task {
    interface IRow<T> {
        int id {
            get;
        }
        T this[string _property] {
            get;
            set;
        }
        void Drop();
    }
}
