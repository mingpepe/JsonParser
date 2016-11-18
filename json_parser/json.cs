using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Mingpepe.Json
{
    public enum JSON_TYPE
    {
        NULL,
        TRUE,
        FALSE,
        NUMBER,
        STRING,
        ARRAY,
        OBJECT,
    };

    public enum PARSE_RESULT
    {
        OK,
        EXPECT_VALUE,
        INVALID_VALUE,
        ROOT_NOT_SINGULAR,
        NUMBER_TOO_BIG,
        MISS_QUOTATION_MARK,
        INVALID_STRING_ESCAPE,
        INVALID_STRING_CHAR,
        INVALID_UNICODE_HEX,
        INVALID_UNICODE_SURROGATE,
        MISS_COMMA_OR_SQUARE_BRACKET,
        MISS_KEY,
        MISS_COLON,
        MISS_COMMA_OR_CURLY_BRACKET,
    };


    public class Value
    {
        private string str;
        private double num;
        private List<Value> arr = new List<Value>();
        private List<Member> member = new List<Member>();

        public string Str
        {
            get
            {
                if (type == JSON_TYPE.STRING)
                {
                    return str;
                }
                throw new InvalidOperationException();
            }
            set
            {
                type = JSON_TYPE.STRING;
                str = value;
            }
        }
        public double Num
        {
            get
            {
                if (type == JSON_TYPE.NUMBER)
                {
                    return num;
                    
                }
                throw new InvalidOperationException();
            }
            set
            {
                type = JSON_TYPE.NUMBER;
                num = value;
            }
        }
        public JSON_TYPE type { get; set; }
        public List<Value> Arr
        {
            get
            {
                if (type == JSON_TYPE.ARRAY)
                {
                    return arr;
                    
                }
                throw new InvalidOperationException();
            }
            set
            {
                type = JSON_TYPE.ARRAY;
                arr = value;
            }
        }
        public List<Member> Member
        {
            get
            {
                if (type == JSON_TYPE.OBJECT)
                {
                    return member;
                }
                throw new InvalidOperationException();
            }
            set
            {
                type = JSON_TYPE.OBJECT;
                member = value;
            }
        }
        public bool Bool
        {
            get
            {
                if (type == JSON_TYPE.TRUE) return true;
                if (type == JSON_TYPE.FALSE) return false;
                throw new InvalidOperationException();
            }
            set
            {
                type = value ? JSON_TYPE.TRUE : JSON_TYPE.FALSE;
            }
        }

        public Value()
        {
            type = JSON_TYPE.NULL;
        }
    }

    public class Member
    {
        public string Key;
        public Value Val;

        public Member()
        {
            Val = new Value();
        }
    }

    public static class JsonParser
    {
        private class Context
        {
            public string json;
            public Stack<object> stack = new Stack<object>();
            public int index = 0;

            public Context(string json)
            {
                this.json = json;
            }
        }

        public static PARSE_RESULT Parse(Value v, string json)
        {
            if (v == null || json == null) throw new ArgumentNullException();

            Context c = new Context(json);
            ParseWhiteSpace(c);
            PARSE_RESULT ret = ParseValue(c, v);
            if (ret == PARSE_RESULT.OK)
            {
                ParseWhiteSpace(c);
                if (c.index != c.json.Length)
                {
                    v.type = JSON_TYPE.NULL;
                    return PARSE_RESULT.ROOT_NOT_SINGULAR;
                }
            }
            return ret;
        }

        private static PARSE_RESULT ParseValue(Context c, Value v)
        {
            if (c.index == c.json.Length) return PARSE_RESULT.EXPECT_VALUE;

            switch (c.json[c.index])
            {
                case 't':
                    return ParseLiteral(c, v, "true", JSON_TYPE.TRUE);
                case 'f':
                    return ParseLiteral(c, v, "false", JSON_TYPE.FALSE);
                case 'n':
                    return ParseLiteral(c, v, "null", JSON_TYPE.NULL);
                case '"':
                    return ParseString(c, v);
                case '[':
                    return ParseArray(c, v);
                case '{':
                    return ParseObject(c, v);
                default:
                    return ParseNumber(c, v);
            }
        }

        private static PARSE_RESULT ParseLiteral(Context c, Value v, string literal, JSON_TYPE type)
        {
            for (int i = 0; i < literal.Length; i++)
            {
                if (c.index + i >= c.json.Length || c.json[c.index + i] != literal[i])
                {
                    return PARSE_RESULT.INVALID_VALUE;
                }
            }
            c.index += literal.Length;
            v.type = type;
            return PARSE_RESULT.OK;
        }

        private static PARSE_RESULT ParseNumber(Context c, Value v)
        {
            int index = c.index;
            if (c.json[c.index] == '-') c.index++;
            if (c.json[c.index] == '0') c.index++;
            else
            {
                if (!IsDigit1To9(c.json[c.index])) return PARSE_RESULT.INVALID_VALUE;
                for (c.index++; c.index < c.json.Length && IsNumeric(c.json[c.index]); c.index++) ;
            }
            if (c.index >= c.json.Length) goto convert;

            if (c.json[c.index] == '.')
            {
                c.index++;
                if (c.index >= c.json.Length) goto convert;
                if (!IsNumeric(c.json[c.index])) return PARSE_RESULT.INVALID_VALUE;
                for (c.index++; c.index < c.json.Length && IsNumeric(c.json[c.index]); c.index++) ;
            }

            if (c.index >= c.json.Length) goto convert;

            if (c.json[c.index] == 'e' || c.json[c.index] == 'E')
            {
                c.index++;
                if (c.index >= c.json.Length) goto convert;

                if (c.json[c.index] == '+' || c.json[c.index] == '-') c.index++;
                if (!IsNumeric(c.json[c.index])) return PARSE_RESULT.INVALID_VALUE;
                for (c.index++; c.index < c.json.Length && IsNumeric(c.json[c.index]); c.index++) ;
            }
            convert:
            try
            {
                v.Num = Convert.ToDouble(c.json.Substring(index, c.index - index));
                return PARSE_RESULT.OK;
            }
            catch (OverflowException)
            {
                return PARSE_RESULT.NUMBER_TOO_BIG;
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Unknown exception : {0}", ex.Message);
                return PARSE_RESULT.INVALID_VALUE;
            }
        }

        private static PARSE_RESULT ParseString(Context c, Value v)
        {
            string str = null;
            PARSE_RESULT ret = ParseString_Internal(c, ref str);
            if (ret == PARSE_RESULT.OK) v.Str = str;
            return ret;
        }

        private static PARSE_RESULT ParseString_Internal(Context c, ref string str)
        {
            // Does not support escape char and Unicode
            if (c.json[c.index++] != '\"') return PARSE_RESULT.INVALID_STRING_CHAR;

            int index = c.index;
            char tmp;
            while (c.index < c.json.Length)
            {
                if (c.index == c.json.Length)
                {
                    return PARSE_RESULT.MISS_QUOTATION_MARK;
                }
                tmp = c.json[c.index++];
                if (tmp == '\"')
                {
                    str = c.json.Substring(index, c.index - index - 1);
                    return PARSE_RESULT.OK;
                }
            }
            return PARSE_RESULT.MISS_QUOTATION_MARK;
        }

        private static void ParseWhiteSpace(Context c)
        {
            char tmp;
            while (c.index < c.json.Length)
            {
                tmp = c.json[c.index];
                if (tmp == ' ' || tmp == '\t' || tmp == '\n' || tmp == '\r') c.index++;
                else return;
            }
        }

        private static PARSE_RESULT ParseArray(Context c, Value v)
        {
            c.index++;
            ParseWhiteSpace(c);
            if (c.json[c.index] == ']')
            {
                c.index++;
                v.type = JSON_TYPE.ARRAY;
                return PARSE_RESULT.OK;
            }

            int count = 0;
            PARSE_RESULT ret;
            while (true)
            {
                Value e = new Value();
                if ((ret = ParseValue(c, e)) != PARSE_RESULT.OK) return ret;

                c.stack.Push(e);
                count++;
                ParseWhiteSpace(c);
                if (c.index < c.json.Length && c.json[c.index] == ',')
                {
                    c.index++;
                    ParseWhiteSpace(c);
                }
                else if (c.index < c.json.Length && c.json[c.index] == ']')
                {
                    c.index++;
                    v.type = JSON_TYPE.ARRAY;
                    for (int i = 0; i < count; i++)
                    {
                        v.Arr.Add((Value)c.stack.Pop());
                    }
                    v.Arr.Reverse();
                    return PARSE_RESULT.OK;
                }
                else return PARSE_RESULT.MISS_COMMA_OR_SQUARE_BRACKET;
            }
        }

        private static PARSE_RESULT ParseObject(Context c, Value v)
        {
            c.index++;
            ParseWhiteSpace(c);
            if (c.json[c.index] == '}')
            {
                c.index++;
                v.type = JSON_TYPE.OBJECT;
                return PARSE_RESULT.OK;
            }

            int count = 0;
            PARSE_RESULT ret;
            while (true)
            {
                if (c.index >= c.json.Length || c.json[c.index] != '"') return PARSE_RESULT.MISS_KEY;
                
                Member m = new Member();
                // Key
                if ((ret = ParseString_Internal(c, ref m.Key)) != PARSE_RESULT.OK) return ret;

                ParseWhiteSpace(c);
                // Colon
                if (c.index >= c.json.Length || c.json[c.index] != ':') return PARSE_RESULT.MISS_COLON;

                c.index++;
                ParseWhiteSpace(c);
                // Value
                if ((ret = ParseValue(c, m.Val)) != PARSE_RESULT.OK) return ret;
                c.stack.Push(m);
                count++;
                ParseWhiteSpace(c);
                if (c.index < c.json.Length && c.json[c.index] == ',')
                {
                    c.index++;
                    ParseWhiteSpace(c);
                }
                else if (c.index < c.json.Length && c.json[c.index] == '}')
                {
                    c.index++;
                    v.type = JSON_TYPE.OBJECT;
                    for (int i = 0; i < count; i++)
                    {
                        v.Member.Add((Member)c.stack.Pop());
                    }
                    v.Member.Reverse();
                    return PARSE_RESULT.OK;
                }
                else return PARSE_RESULT.MISS_COMMA_OR_CURLY_BRACKET;
            }
        }

        private static bool IsDigit1To9(char c)
        {
            return '1' <= c && c <= '9';
        }

        private static bool IsNumeric(char c)
        {
            return '0' <= c && c <= '9';
        }
    }
}