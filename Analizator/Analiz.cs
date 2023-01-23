using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analizator
{
    class Analiz
    {
        private enum States
        {
            S, E, F, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10,
            A11, A12, A13, A14, A15, A16, A17, A18, A19, A20,
            A21, A22, A23, A24, A25, A26, A27, A28, A29, A30,
            A31, A32, A33, A34, A35, A36, A37, A38, A39, A40,
            A41, A42, A43, A44, A45
        };

        // массив зарезервированных строк
        private static string[] RESERVED_WORDS = { "FOR", "TO", "DOWNTO", "DIV", "MOD", "DO" };

        // позиция курсора
        private static int pos;

        // исходная строка
        private static string str;

        // перечисление ошибок
        private static Err err;

        // флаг для проверки выражения конечного значения
        private static bool flag;


        // Позиция курсора ошибки в анализируемой строке (-1, если все корректно)
        private static int errPos;

        // Списки идентификаторов и констант
        private static LinkedList<string> listConst;
        private static LinkedList<string> listId;

        // кол-во итераций цикла, если границы цикла - числа (иначе, -1)
        private static int countOfIteration;

        // Установка типа и позиции ошибки
        private static void SetError(Err errorType, int errorPosition)
        {
            err = errorType;
            errPos = errorPosition;
        }

        // Функция, реализующая проверку
        public static Result Check(string inputString)
        {
            pos = 0;
            str = inputString;
            listConst = new LinkedList<string>();
            listId = new LinkedList<string>();
            countOfIteration = -1;

            SetError(Err.NoError, -1);
            isCorrect();
            return new Result(errPos, err, listId, listConst, countOfIteration);
        }

        // Реализация конечного автомата
        private static bool isCorrect()
        {
            States state = States.S;

            // храним текущие идентификаторы и константы
            string id = "", constant = "";
            int count1 = 0;
            int count2 = 0;

            // Переменные для подсчета числа итераций цикла
            string initExpr = "", finalExpr = "", cycleOperator = "";

            while ((state != States.F) && (state != States.E))
            {
                if (pos >= str.Length)
                {
                    state = States.E;
                    SetError(Err.OutOfRange, pos);
                }
                else
                {
                    switch (state)
                    {
                        // оператор FOR 
                        // ожидаем букву "f"
                        case States.S:
                            if (str[pos] == ' ') { }
                            else if (Char.ToLower(str[pos]) == 'f')
                            {
                                state = States.A1;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedFOR, pos);
                            }
                            break;
                        // ожидаем букву "о"
                        case States.A1:
                            if (Char.ToLower(str[pos]) == 'o')
                            {
                                state = States.A2;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedFOR, pos);
                            }
                            break;
                        // ожидаем букву "r"
                        case States.A2:
                            if (Char.ToLower(str[pos]) == 'r')
                            {
                                state = States.A3;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedFOR, pos);
                            }
                            break;
                        // ожидаем букву обязательный пробел
                        case States.A3:
                            if (str[pos] == ' ')
                            {
                                state = States.A4;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedFOR, pos);
                            }
                            break;

                        // идентификатор
                        // ожидаем либо необязательный пробел, либо букву
                        case States.A4:
                            if (str[pos] == ' ') { }
                            else if (Char.IsLetter(str[pos]) || str[pos] == '_')
                            {
                                state = States.A5;
                                id += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOr_Expected, pos);
                            }
                            break;
                        // ожидаем либо пробел, означающий конец ввода идентификатора,
                        // либо букву/цифру/_, либо двоеточие
                        case States.A5:
                            if (Char.IsLetterOrDigit(str[pos]) || str[pos] == '_')
                            {
                                id += str[pos];
                            }
                            else if (str[pos] == ' ')
                            {
                                if (isCorrectId(id))
                                {
                                    state = States.A6;
                                    if (!listId.Contains(id))
                                    {
                                        listId.AddLast(id.ToLower());
                                        listId.AddLast(" - счетчик цикла");
                                    }

                                    id = "";
                                }
                                else
                                {
                                    state = States.E;
                                }
                            }
                            else if (str[pos] == ':')
                            {
                                if (isCorrectId(id))
                                {
                                    state = States.A7;
                                    if (!listId.Contains(id))
                                    {
                                        listId.AddLast(id);
                                        listId.AddLast(" - счетчик цикла");
                                    }


                                    id = "";
                                }
                                else
                                {
                                    state = States.E;
                                }
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.InvalidIdSymbol, pos);
                            }
                            break;
                        // ожидаем либо необязательный пробел, либо двоеточие
                        case States.A6:
                            if (str[pos] == ' ') { }
                            else if (str[pos] == ':')
                            {
                                state = States.A7;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.AssignmentExpected, pos);
                            }
                            break;
                        // ожидаем знак "="
                        case States.A7:
                            if (str[pos] == '=')
                            {
                                state = States.A8;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.AssignmentExpected, pos);
                            }
                            break;

                        // начальное значение
                        // ожидаем либо необязательный пробел, либо букву/_, либо цифру 
                        case States.A8:
                            if (str[pos] == ' ') { }
                            else if (str[pos] == '-' || str[pos] == '+')
                            {
                                state = States.A9;
                                constant += str[pos];
                            }
                            else if (str[pos] == '0')
                            {
                                state = States.A12;
                                constant += str[pos];
                                listConst.AddLast(constant.ToLower());
                                listConst.AddLast(" - начальное значение");

                            }
                            else if (Char.IsLetter(str[pos]) || str[pos] == '_')
                            {
                                state = States.A10;
                                id += str[pos];
                            }
                            else if (Char.IsDigit(str[pos]))
                            {
                                state = States.A11;
                                constant += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOrDigitOr_OrMinusExpected, pos);
                            }

                            break;
                        // ожидаем цифру
                        case States.A9:
                            if (Char.IsDigit(str[pos]))
                            {
                                state = States.A11;
                                constant += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DigitExpected, pos);
                            }
                            break;
                        // ожидаем либо пробел, означающий конец ввода идентификатора,
                        // либо букву/цифру/_
                        case States.A10:
                            if (Char.IsLetterOrDigit(str[pos]) || str[pos] == '_')
                            {
                                id += str[pos];
                            }
                            else if (str[pos] == ' ')
                            {
                                if (isCorrectId(id))
                                {
                                    state = States.A12;
                                    if (!listId.Contains(id))
                                    {
                                        listId.AddLast(id.ToLower());
                                        listId.AddLast(" - начальное значение");
                                    }

                                    initExpr = id;
                                    id = "";
                                }
                                else
                                {
                                    state = States.E;
                                }
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOrDigitOr_Expected, pos);
                            }
                            break;
                        // ожидаем либо пробел, означающий конец ввода константы, либо цифру
                        case States.A11:
                            if (Char.IsDigit(str[pos]))
                            {
                                constant += str[pos];
                            }
                            else if (str[pos] == ' ')
                            {

                                if (isCorrectInt(constant, false))
                                {
                                    state = States.A12;
                                    if (!listConst.Contains(constant))
                                    {
                                        listConst.AddLast(Int32.Parse(constant).ToString());
                                        listConst.AddLast(" - начальное значение");
                                    }

                                    initExpr = constant;
                                    constant = "";
                                }
                                else
                                {
                                    state = States.E;
                                    SetError(Err.InvalidIntValue, pos - constant.Length);
                                }
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DigitExpected, pos);
                            }

                            break;

                        // оператор TO/DOWNTO
                        // ожидаем либо "d", либо "t"
                        case States.A12:
                            if (str[pos] == ' ') { }
                            else if (Char.ToLower(str[pos]) == 'd')
                            {
                                state = States.A13;
                                cycleOperator += 'D';
                            }
                            else if (Char.ToLower(str[pos]) == 't')
                            {
                                state = States.A17;
                                cycleOperator += 'T';
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedTOorDOWNTO, pos);
                            }
                            break;
                        // ожидаем "о"
                        case States.A13:
                            if (Char.ToLower(str[pos]) == 'o')
                            {
                                state = States.A14;
                                cycleOperator += 'O';
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedTOorDOWNTO, pos);
                            }
                            break;
                        // ожидаем "w"
                        case States.A14:
                            if (Char.ToLower(str[pos]) == 'w')
                            {
                                state = States.A15;
                                cycleOperator += 'W';
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedTOorDOWNTO, pos);
                            }
                            break;
                        // ожидаем "n"
                        case States.A15:
                            if (Char.ToLower(str[pos]) == 'n')
                            {
                                state = States.A16;
                                cycleOperator += 'N';
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedTOorDOWNTO, pos);
                            }
                            break;
                        // ожидаем "t"
                        case States.A16:
                            if (Char.ToLower(str[pos]) == 't')
                            {
                                state = States.A17;
                                cycleOperator += 'T';
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedTOorDOWNTO, pos);
                            }
                            break;
                        // ожидаем "o"
                        case States.A17:
                            if (Char.ToLower(str[pos]) == 'o')
                            {
                                state = States.A18;
                                cycleOperator += 'O';
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedTOorDOWNTO, pos);
                            }
                            break;
                        // ожидаем обязательный пробел
                        case States.A18:
                            if (str[pos] == ' ')
                            {
                                state = States.A19;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.ExpectedTOorDOWNTO, pos);
                            }

                            break;

                        // конечное значение (либо терм со знаком, либо без)
                        case States.A19:
                            if (str[pos] == ' ') { }
                            else if (Char.IsLetter(str[pos]) || str[pos] == '_')
                            {
                                state = States.A20;
                                id += str[pos];
                                finalExpr += str[pos];
                            }
                            else if (str[pos] == '+' || str[pos] == '-')
                            {
                                state = States.A21;
                                finalExpr += str[pos];
                            }
                            else if (Char.IsDigit(str[pos]))
                            {
                                state = States.A22;
                                constant += str[pos];
                                finalExpr += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOrDigitOr_OrMinusExpected, pos);
                            }
                            break;

                        // ожидаем букву, цифру, _, мат. операцию, пробел
                        case States.A20:
                            if (Char.IsLetterOrDigit(str[pos]) || str[pos] == '_')
                            {
                                id += str[pos];
                                finalExpr += str[pos];
                            }
                            else if (isMatOper(str[pos]))
                            {
                                // for k:= 6 to g - t do c:= 9 - 0;
                                if (isCorrectId(id))
                                {
                                    state = States.A23;
                                    if (!listId.Contains(id))
                                    {
                                        listId.AddLast(id);
                                        if (flag && count2 == 1)
                                        {
                                            listConst.Remove(" - конечное значение");
                                            listConst.AddLast(" - константа");
                                            listId.AddLast(" - переменная");
                                        }
                                        else if (flag)
                                        {
                                            listId.Remove(" - конечное значение");
                                            listId.AddBefore(listId.FindLast(id), " - переменная");
                                            listId.AddLast(" - переменная");


                                        }

                                        else
                                        {
                                            listId.AddLast(" - конечное значение");
                                            count1++;

                                        }
                                        flag = true;

                                    }
                                    id = "";
                                    finalExpr += str[pos];
                                }
                                else
                                {
                                    state = States.E;
                                }
                            }

                            else if (str[pos] == ' ')
                            {
                                if (isCorrectId(id))
                                {
                                    state = States.A24;
                                    if (listId.Find(id) == null)
                                    {
                                        listId.AddLast(id.ToLower());
                                        if (flag && count2 == 1)
                                        {
                                            listConst.Remove(" - конечное значение");
                                            listConst.AddLast(" - константа");
                                            listId.AddLast(" - переменная");
                                        }
                                        else if (flag)
                                        {
                                            listId.Remove(" - конечное значение");
                                            listId.AddBefore(listId.FindLast(id), " - переменная");
                                            listId.AddLast(" - переменная");


                                        }
                                        else
                                        {
                                            listId.AddLast(" - конечное значение");
                                            count1++;
                                        }


                                    }

                                    id = "";
                                    finalExpr += str[pos];
                                }
                                else
                                {
                                    state = States.E;
                                }
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.InvalidIdSymbol, pos);
                            }
                            break;
                        // ожидаем букву, _, цифру
                        case States.A21:
                            if (Char.IsLetter(str[pos]) || str[pos] == '_')
                            {
                                state = States.A20;
                                id += str[pos];
                                finalExpr += str[pos];
                            }
                            else if (Char.IsDigit(str[pos]))
                            {
                                state = States.A22;
                                constant += str[pos - 1];
                                constant += str[pos];
                                finalExpr += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOrDigitOr_Expected, pos);
                            }
                            break;
                        // ожидаем цифру, мат. операцию, пробел
                        case States.A22:
                            if (Char.IsDigit(str[pos]))
                            {
                                constant += str[pos];
                                finalExpr += str[pos];
                            }
                            else if (isMatOper(str[pos]))
                            {
                                if (isCorrectInt(constant, true))
                                {
                                    state = States.A23;
                                    if (!listConst.Contains(constant))
                                    {
                                        listConst.AddLast(Int32.Parse(constant).ToString());
                                        if (flag && count1 == 1)
                                        {
                                            listId.Remove(" - конечное значение");
                                            listId.AddLast(" - переменная");
                                            listConst.AddLast(" - константа");
                                        }
                                        else if (flag)
                                        {
                                            listConst.Remove(" - конечное значение");
                                            listConst.AddBefore(listConst.Find(constant), "- константа");
                                            listConst.AddLast(" - константа");

                                        }


                                        else
                                        {
                                            listConst.AddLast(" - конечное значение");
                                            count2++;

                                        }
                                        flag = true;

                                    }


                                    constant = "";
                                    finalExpr += str[pos];
                                }
                                else
                                {
                                    state = States.E;
                                    SetError(Err.InvalidIntValue, pos - constant.Length);
                                }
                            }
                            else if (str[pos] == ' ')
                            {
                                if (isCorrectInt(constant, true))
                                {
                                    state = States.A24;

                                    if (!listConst.Contains(constant))
                                    {
                                        listConst.AddLast(Int32.Parse(constant).ToString());
                                        if (flag && count1 == 1)
                                        {
                                            listId.Remove(" - конечное значение");
                                            listId.AddLast(" - переменная");
                                            listConst.AddLast(" - константа");
                                        }
                                        else if (flag)
                                        {
                                            listConst.Remove(" - конечное значение");
                                            listConst.AddBefore(listConst.Find(constant), "- константа");
                                            listConst.AddLast(" - константа");

                                        }



                                        else
                                        {
                                            listConst.AddLast(" - конечное значение");
                                            count2++;
                                        }

                                    }

                                    constant = "";
                                    finalExpr += str[pos];
                                }
                                else
                                {
                                    state = States.E;
                                    SetError(Err.InvalidIntValue, pos - constant.Length);
                                }
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.InvalidIntValue, pos);
                            }
                            break;
                        // ожидаем пробел, цифру
                        case States.A23:
                            if (str[pos] == ' ') { }
                            else if (Char.IsLetter(str[pos]) || str[pos] == '_')
                            {
                                state = States.A20;
                                id += str[pos];
                                finalExpr += str[pos];

                            }
                            else if (Char.IsDigit(str[pos]))
                            {
                                state = States.A22;
                                constant += str[pos];
                                finalExpr += str[pos];

                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOrDigitOr_Expected, pos);
                            }
                            break;
                        // ожидаем пробел, мат. операцию, 'm', 'd'
                        case States.A24:
                            if (str[pos] == ' ') { }
                            else if (isMatOper(str[pos]))
                            {
                                state = States.A23;
                                finalExpr += str[pos];
                                flag = true;

                            }
                            else if (Char.ToLower(str[pos]) == 'm')
                            {
                                state = States.A25;
                                finalExpr += str[pos];
                            }
                            else if (Char.ToLower(str[pos]) == 'd')
                            {
                                state = States.A27;
                                finalExpr += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DOorMatOperExpected, pos);
                            }

                            break;
                        // ожидаем 'o'
                        case States.A25:
                            if (Char.ToLower(str[pos]) == 'o')
                            {
                                state = States.A26;
                                finalExpr += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DOorMatOperExpected, pos - 1);
                            }
                            break;
                        // ожидаем 'd'
                        case States.A26:
                            if (Char.ToLower(str[pos]) == 'd')
                            {
                                state = States.A29;
                                finalExpr += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DOorMatOperExpected, pos - 2);
                            }
                            break;
                        // ожидаем 'i', 'o'
                        case States.A27:
                            if (Char.ToLower(str[pos]) == 'i')
                            {
                                state = States.A28;
                                finalExpr += str[pos];
                            }
                            else if (Char.ToLower(str[pos]) == 'o')
                            {
                                state = States.A30;
                                finalExpr = finalExpr.Substring(0, finalExpr.Length - 2);
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DOorMatOperExpected, pos - 1);
                            }
                            break;
                        // ожидаем 'v'
                        case States.A28:
                            if (Char.ToLower(str[pos]) == 'v')
                            {
                                state = States.A29;
                                finalExpr += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DOorMatOperExpected, pos - 2);
                            }
                            break;
                        // ожидаем пробел
                        case States.A29:
                            if (str[pos] == ' ')
                            {
                                state = States.A23;
                                finalExpr += str[pos];

                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DOorMatOperExpected, pos - 3);
                            }
                            break;
                        // ожидаем пробел
                        case States.A30:
                            if (str[pos] == ' ')
                            {
                                state = States.A31;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DOorMatOperExpected, pos - 2);
                            }
                            break;

                        // оператор присваивания
                        case States.A31:
                            if (str[pos] == ' ') { }
                            else if (Char.IsLetter(str[pos]) || str[pos] == '_')
                            {
                                state = States.A32;
                                id += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOr_Expected, pos);
                            }
                            break;
                        // ожидаем букву/цифру, пробел, двоеточие
                        case States.A32:
                            if (Char.IsLetterOrDigit(str[pos]) || str[pos] == '_')
                            {
                                id += str[pos];
                            }
                            else if (str[pos] == ' ')
                            {
                                if (isCorrectId(id))
                                {
                                    state = States.A33;
                                    if (!listId.Contains(id))
                                    {

                                    }

                                    id = "";
                                }
                                else
                                {
                                    state = States.E;
                                }
                            }
                            else if (str[pos] == ':')
                            {
                                if (isCorrectId(id))
                                {
                                    state = States.A34;
                                    if (!listId.Contains(id))
                                    {
                                        listId.AddLast(id.ToLower());
                                        listId.AddLast(" - переменная");
                                    }

                                    id = "";
                                }
                                else
                                {
                                    state = States.E;
                                }
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.InvalidIdSymbol, pos);
                            }
                            break;
                        // ожидаем пробел, двоеточие
                        case States.A33:
                            if (str[pos] == ' ') { }
                            else if (str[pos] == ':')
                            {
                                state = States.A34;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.AssignmentExpected, pos);
                            }
                            break;
                        // ожидаем знак равенства
                        case States.A34:
                            if (str[pos] == '=')
                            {
                                state = States.A35;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.AssignmentExpected, pos);
                            }
                            break;
                        // ожидаем пробел, _, букву, +/-, цифру
                        case States.A35:
                            if (str[pos] == ' ') { }
                            else if (Char.IsLetter(str[pos]) || str[pos] == '_')
                            {
                                state = States.A36;
                                id += str[pos];
                            }
                            else if (str[pos] == '+' || str[pos] == '-')
                            {
                                state = States.A37;
                            }
                            else if (Char.IsDigit(str[pos]))
                            {
                                state = States.A38;
                                constant += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOrDigitOr_OrMinusExpected, pos);
                            }
                            break;
                        // ожидаем букву/цифру/_, мат. операцию, пробел, ;
                        case States.A36:
                            if (Char.IsLetterOrDigit(str[pos]) || str[pos] == '_')
                            {
                                id += str[pos];
                            }
                            else if (isMatOper(str[pos]))
                            {
                                if (isCorrectId(id))
                                {
                                    state = States.A39;
                                    if (!listId.Contains(id))
                                    {
                                        listId.AddLast(id);
                                        listId.AddLast(" - переменная");
                                    }

                                    id = "";
                                }
                                else
                                {
                                    state = States.E;
                                }
                            }
                            else if (str[pos] == ' ')
                            {
                                if (isCorrectId(id))
                                {
                                    state = States.A40;
                                    if (!listId.Contains(id))
                                    {
                                        listId.AddLast(id.ToLower());
                                        listId.AddLast(" - переменная");
                                    }

                                    id = "";
                                }
                                else
                                {
                                    state = States.E;
                                }
                            }
                            else if (str[pos] == ';')
                            {
                                if (isCorrectId(id))
                                {
                                    state = States.F;
                                    if (!listId.Contains(id))
                                    {
                                        listId.AddLast(id.ToLower());
                                        listId.AddLast(" - переменная");
                                    }

                                }
                                else
                                {
                                    state = States.E;
                                }
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.InvalidIdSymbol, pos);
                            }
                            break;
                        // ожидаем букву, цифру, _
                        case States.A37:
                            if (Char.IsLetter(str[pos]) || str[pos] == '_')
                            {
                                state = States.A36;
                                id += str[pos];
                            }
                            else if (Char.IsDigit(str[pos]))
                            {
                                state = States.A38;
                                constant += str[pos - 1];
                                constant += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOrDigitOr_Expected, pos);
                            }
                            break;
                        // ожидаем цифру, мат. операцию, пробел, ;
                        case States.A38:
                            if (Char.IsDigit(str[pos]))
                            {
                                constant += str[pos];
                            }
                            else if (isMatOper(str[pos]))
                            {
                                if (isCorrectInt(constant, true))
                                {
                                    state = States.A39;
                                    if (!listConst.Contains(constant))
                                        listConst.AddLast(Int32.Parse(constant).ToString());
                                    constant = "";
                                }
                                else
                                {
                                    state = States.E;
                                    SetError(Err.InvalidIntValue, pos - constant.Length);
                                }
                            }
                            else if (str[pos] == ' ')
                            {
                                if (isCorrectInt(constant, true))
                                {
                                    state = States.A40;

                                    if (!listConst.Contains(constant))
                                    {
                                        listConst.AddLast(Int32.Parse(constant).ToString());
                                        listConst.AddLast(" - константа");
                                    }

                                    constant = "";
                                }
                                else
                                {
                                    state = States.E;
                                    SetError(Err.InvalidIntValue, pos - constant.Length);
                                }
                            }
                            else if (str[pos] == ';')
                            {
                                if (isCorrectInt(constant, true))
                                {
                                    state = States.F;

                                    if (!listConst.Contains(constant))
                                    {
                                        listConst.AddLast(Int32.Parse(constant).ToString());
                                        listConst.AddLast(" - константа");
                                    }

                                }
                                else
                                {
                                    state = States.E;
                                    SetError(Err.InvalidIntValue, pos - constant.Length);
                                }
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.DigitExpected, pos);
                            }
                            break;
                        // ожидаем пробел, букву/_, цифру
                        case States.A39:
                            if (str[pos] == ' ') { }
                            else if (Char.IsLetter(str[pos]) || str[pos] == '_')
                            {
                                state = States.A36;
                                id += str[pos];
                            }
                            else if (Char.IsDigit(str[pos]))
                            {
                                state = States.A38;
                                constant += str[pos];
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.LetterOrDigitOr_Expected, pos);
                            }
                            break;
                        // ожидаем пробел, 'm', 'd', ;
                        case States.A40:
                            if (str[pos] == ' ') { }
                            else if (isMatOper(str[pos]))
                            {
                                state = States.A39;
                            }
                            else if (Char.ToLower(str[pos]) == 'm')
                            {
                                state = States.A41;
                            }
                            else if (Char.ToLower(str[pos]) == 'd')
                            {
                                state = States.A43;
                            }
                            else if (str[pos] == ';')
                            {
                                state = States.F;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.MatOperExpected, pos);
                            }
                            break;
                        // ожидаем 'o'
                        case States.A41:
                            if (Char.ToLower(str[pos]) == 'o')
                            {
                                state = States.A42;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.MatOperExpected, pos - 1);
                            }
                            break;
                        // ожидаем 'd'
                        case States.A42:
                            if (Char.ToLower(str[pos]) == 'd')
                            {
                                state = States.A45;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.MatOperExpected, pos - 2);
                            }
                            break;
                        // ожидаем 'i'
                        case States.A43:
                            if (Char.ToLower(str[pos]) == 'i')
                            {
                                state = States.A44;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.MatOperExpected, pos - 1);
                            }
                            break;
                        // ожидаем 'v'
                        case States.A44:
                            if (Char.ToLower(str[pos]) == 'v')
                            {
                                state = States.A45;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.MatOperExpected, pos - 2);
                            }
                            break;
                        // ожидаем пробел
                        case States.A45:
                            if (str[pos] == ' ')
                            {
                                state = States.A39;
                            }
                            else
                            {
                                state = States.E;
                                SetError(Err.MatOperExpected, pos - 3);
                            }
                            break;
                    }
                }
                pos++;
            }
            flag = false;

            // вычисляем число итераций цикла
            countOfIteration = CountOfIterations(initExpr, finalExpr, cycleOperator);

            if (countOfIteration == -2)
            {
                state = States.E;
                SetError(Err.StartGreaterFinish, pos);
            }
            else if (countOfIteration == -3)
            {
                state = States.E;
                SetError(Err.FinishGreaterStart, pos);
            }


            return (state == States.F);
        }

        // Проверка идентификатора на синтаксические ограничения
        private static bool isCorrectId(string value)
        {
            if (value.Length > 8)
            {
                SetError(Err.InvalidIdLength, pos - value.Length);
                return false;
            }

            if (RESERVED_WORDS.Contains(value.ToUpper()))
            {
                SetError(Err.ReservedWord, pos - value.Length);
                return false;
            }

            return true;
        }

        // проверка константы на синтаксические ограничения
        // _uint == true => беззнаковая константа. Иначе, знаковая
        private static bool isCorrectInt(string value, bool _uint)
        {
            if (_uint)
                return ushort.TryParse(value, out ushort val);
            else
                return short.TryParse(value, out short val);
        }

        // является ли символ знаком математической операции
        private static bool isMatOper(char value)
        {
            return str[pos] == '-' || str[pos] == '+' ||
                   str[pos] == '*' || str[pos] == '/';
        }

        // метод считает число итераций цикла
        private static int CountOfIterations(string begin, string end, string cycleOperator)
        {
            if (cycleOperator != "TO" && cycleOperator != "DOWNTO")
                return -1;
            int start, finish;
            if (Int32.TryParse(begin, out start) && Int32.TryParse(end, out finish))
            {
                if (cycleOperator == "TO")
                {
                    if (finish > start)
                        return (finish - start < 0) ? 0 : finish - start + 1;
                    else if (start == finish)
                        return 1;
                    else
                        return -2;
                }
                else if (cycleOperator == "DOWNTO")
                {
                    if (start > finish)
                        return (start - finish < 0) ? 0 : start - finish + 1;
                    else if (start == finish)
                        return 1;
                    else
                        return -3;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }
    }
}
