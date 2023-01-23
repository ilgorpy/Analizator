using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analizator
{
    // Перечисление с типами ошибок
    public enum Err
    {
        NoError,        // нет ошибок
        OutOfRange,     // выход за границы анализируемой строки

        AssignmentExpected,     // ожидался оператор присвоения
        ExpectedFOR,            // ожидалось зарезервированное слово FOR 
        ExpectedTOorDOWNTO,     // ожидалось зарезервированное слово TO или DOWNTO
        ReservedWord,           // зарезервированное слово

        LetterOr_Expected,                  // ожидалась буква или подчеркивание
        LetterOrDigitOr_Expected,           // ожидалось буква, цифра или подчеркивание
        LetterOrDigitOr_OrMinusExpected,    // ожидалось буква, цифра, подчеркивание или минус
        DigitExpected,                      // ожидалась цифра
        DOorMatOperExpected,                // ожидалось математическая операция или DO
        MatOperExpected,                    // ожидалось математическая операция
        
        InvalidIdLength,    // превышена допустимая длина идентификатора
        InvalidIntValue,    // недопустимое значение константы
        InvalidIdSymbol,     // недопустимый символ в имени идентификатора
        StartGreaterFinish,   // начальное значение больше конечного
        FinishGreaterStart,   // конечное значение больше начального
        InsignificantZero    // незначащий ноль
    }

    // Класс для передачи результата в интерфейс пользователя
    public class Result
    {
        // позиция ошибки (-1, если все корректно)
        private int errPosition;

        // тип ошибки
        private Err err;

        // списки идентификаторов и констант
        private LinkedList<string> listId;
        private LinkedList<string> listConst;

        // кол-во итераций цикла, если границы цикла - числа (иначе, -1)
        private int countOfIteration;

        public Result(int errPosition, Err err, LinkedList<string> listId,
            LinkedList<string> listConst, int countOfIteration)
        {
            this.errPosition = errPosition;
            this.err = err;
            this.listId = listId;
            this.listConst = listConst;
            this.countOfIteration = countOfIteration;
        }

        public string ErrMessage 
        {
            get 
            {
                switch(err)
                {
                    case Err.NoError: { return "Нет ошибок."; }
                    case Err.OutOfRange: { return "Ошибка: Выход за границы строки."; }
                    case Err.ExpectedFOR: { return "Ошибка: Ожидалось FOR."; }
                    case Err.LetterOr_Expected: { return "Ошибка: Ожидалась буква или нижнее подчеркивание."; }
                    case Err.InvalidIdSymbol: { return "Ошибка: Недопустимый символ в имени идентификатора."; }
                    case Err.ReservedWord: { return "Ошибка: Недопустимое имя идентификатора"; }
                    case Err.InvalidIdLength: { return "Ошибка: Превышена допустимая длина идентификатора"; }
                    case Err.AssignmentExpected: { return "Ошибка: Ожидался оператор присвоения."; }
                    case Err.LetterOrDigitOr_OrMinusExpected: { return "Ошибка: Ожидалось буква, цифра, подчеркивание или минус."; }
                    case Err.LetterOrDigitOr_Expected: { return "Ошибка: Ожидалось буква, цифра или подчеркивание."; }
                    case Err.InvalidIntValue: { return "Ошибка: Недопустимое значение константы."; }
                    case Err.ExpectedTOorDOWNTO: { return "Ошибка: Ожидалось зарезервированное слово TO или DOWNTO."; }
                    case Err.DOorMatOperExpected: { return "Ошибка: Ожидалось математическая операция или DO."; }
                    case Err.MatOperExpected: { return "Ошибка: Ожидалось математическая операция."; }
                    case Err.DigitExpected: { return "Ошибка: Ожидалась цифра"; }
                    case Err.FinishGreaterStart: { return "Ошибка: Конечное значение больше начального."; }
                    case Err.StartGreaterFinish: { return "Ошибка: Начальное значение больше конечного."; }
                    case Err.InsignificantZero: { return "Ошибка: Незначащий ноль."; }

                    default: { return "Ошибка: Неизвестная ошибка."; }
                }
            }
        }

        #region Свойства доступа к полям класса
        public int ErrPosition
        {
            get
            {
                return errPosition;
            }
        }

        public LinkedList<string> ListId 
        {
            get 
            {
                return listId;
            }
        }

        public LinkedList<string> ListConst
        {
            get
            {
                return listConst;
            }
        }

        public int CountOfIteration
        {
            get 
            {
                return countOfIteration;
            }
        }
        #endregion
    }
}
