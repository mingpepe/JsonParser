using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mingpepe.Json.Tests
{
    [TestClass()]
    public class JsonParserTests
    {
        [TestMethod()]
        public void test_parse_null()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "null"));
            Assert.AreEqual(JSON_TYPE.NULL, v.type);
        }

        [TestMethod()]
        public void test_parse_true()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "true"));
            Assert.AreEqual(JSON_TYPE.TRUE, v.type);

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, " true "));
            Assert.AreEqual(JSON_TYPE.TRUE, v.type);
        }

        [TestMethod()]
        public void test_parse_false()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "false"));
            Assert.AreEqual(JSON_TYPE.FALSE, v.type);

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, " false "));
            Assert.AreEqual(JSON_TYPE.FALSE, v.type);
        }

        [TestMethod()]
        public void test_parse_numnber()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "0"));
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(0, v.Num);

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "-0"));
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(0, v.Num);

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "-0.0"));
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(0, v.Num);

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "1"));
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(1, v.Num);

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "-1"));
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(-1, v.Num);

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "1.5"));
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(1.5, v.Num);

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "-1.5"));
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(-1.5, v.Num);

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "3.1416"));
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(3.1416, v.Num);
        }

        [TestMethod()]
        public void test_parse_string()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "\"Hello\""));
            Assert.AreEqual(JSON_TYPE.STRING, v.type);
            Assert.AreEqual("Hello", v.Str);
        }

        [TestMethod()]
        public void test_parse_array()
        {
            Value v;

            v = new Value();
            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "[]"));
            Assert.AreEqual(JSON_TYPE.ARRAY, v.type);
            Assert.AreEqual(0, v.Arr.Count);

            v = new Value();
            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "[null,false,true,123,\"abc\"]"));
            Assert.AreEqual(5, v.Arr.Count);
            Assert.AreEqual(JSON_TYPE.NULL, v.Arr[0].type);
            Assert.AreEqual(JSON_TYPE.FALSE, v.Arr[1].type);
            Assert.AreEqual(JSON_TYPE.TRUE, v.Arr[2].type);
            Assert.AreEqual(123, v.Arr[3].Num);
            Assert.AreEqual("abc", v.Arr[4].Str);

            v = new Value();
            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, "[[ ], [ 0],[0,1],[0,1,2]]"));
            Assert.AreEqual(4, v.Arr.Count);
            Assert.AreEqual(0, v.Arr[0].Arr.Count);
            Assert.AreEqual(1, v.Arr[1].Arr.Count);
            Assert.AreEqual(2, v.Arr[2].Arr.Count);
            Assert.AreEqual(3, v.Arr[3].Arr.Count);
            Assert.AreEqual(0, v.Arr[1].Arr[0].Num);
            Assert.AreEqual(0, v.Arr[2].Arr[0].Num);
            Assert.AreEqual(1, v.Arr[2].Arr[1].Num);
            Assert.AreEqual(0, v.Arr[3].Arr[0].Num);
            Assert.AreEqual(1, v.Arr[3].Arr[1].Num);
            Assert.AreEqual(2, v.Arr[3].Arr[2].Num);
        }

        [TestMethod()]
        public void test_parse_object()
        {
            Value v;

            v = new Value();
            Assert.AreEqual(PARSE_RESULT.OK, JsonParser.Parse(v, " { " +
                                            "\"n\" : null , " +
                                            "\"f\" : false , " +
                                            "\"t\" : true , " +
                                            "\"i\" : 123 , " +
                                            "\"s\" : \"abc\" , " +
                                            "\"a\" : [ 1, 2, 3 ]," +
                                            "\"o\" : { \"1\" : 1, \"2\" : 2, \"3\" : 3, \"4\" : 4 }" +
                                            " } "));
            Assert.AreEqual(JSON_TYPE.OBJECT, v.type);
            Assert.AreEqual(7, v.Member.Count);

            Assert.AreEqual("n", v.Member[0].Key);
            Assert.AreEqual(JSON_TYPE.NULL, v.Member[0].Val.type);

            Assert.AreEqual("f", v.Member[1].Key);
            Assert.AreEqual(JSON_TYPE.FALSE, v.Member[1].Val.type);

            Assert.AreEqual("t", v.Member[2].Key);
            Assert.AreEqual(JSON_TYPE.TRUE, v.Member[2].Val.type);

            Assert.AreEqual("i", v.Member[3].Key);
            Assert.AreEqual(123, v.Member[3].Val.Num);

            Assert.AreEqual("s", v.Member[4].Key);
            Assert.AreEqual("abc", v.Member[4].Val.Str);

            Assert.AreEqual("a", v.Member[5].Key);
            Assert.AreEqual(3, v.Member[5].Val.Arr.Count);
            Assert.AreEqual(1, v.Member[5].Val.Arr[0].Num);
            Assert.AreEqual(2, v.Member[5].Val.Arr[1].Num);
            Assert.AreEqual(3, v.Member[5].Val.Arr[2].Num);

            Assert.AreEqual("o", v.Member[6].Key);
            Assert.AreEqual("1", v.Member[6].Val.Member[0].Key);
            Assert.AreEqual("2", v.Member[6].Val.Member[1].Key);
            Assert.AreEqual("3", v.Member[6].Val.Member[2].Key);
            Assert.AreEqual("4", v.Member[6].Val.Member[3].Key);

            Assert.AreEqual(1, v.Member[6].Val.Member[0].Val.Num);
            Assert.AreEqual(2, v.Member[6].Val.Member[1].Val.Num);
            Assert.AreEqual(3, v.Member[6].Val.Member[2].Val.Num);
            Assert.AreEqual(4, v.Member[6].Val.Member[3].Val.Num);
        }

        [TestMethod()]
        public void test_parse_expect_value()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.EXPECT_VALUE, JsonParser.Parse(v, ""));
            Assert.AreEqual(PARSE_RESULT.EXPECT_VALUE, JsonParser.Parse(v, " "));
        }

        [TestMethod()]
        public void test_parse_invalid_value()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.INVALID_VALUE, JsonParser.Parse(v, "nul"));
            Assert.AreEqual(PARSE_RESULT.INVALID_VALUE, JsonParser.Parse(v, "?"));
            Assert.AreEqual(PARSE_RESULT.INVALID_VALUE, JsonParser.Parse(v, "+0"));
            Assert.AreEqual(PARSE_RESULT.INVALID_VALUE, JsonParser.Parse(v, ".123"));
        }

        [TestMethod()]
        public void test_parse_root_not_singular()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.ROOT_NOT_SINGULAR, JsonParser.Parse(v, "null x"));
            Assert.AreEqual(PARSE_RESULT.ROOT_NOT_SINGULAR, JsonParser.Parse(v, "0x123"));
            Assert.AreEqual(PARSE_RESULT.ROOT_NOT_SINGULAR, JsonParser.Parse(v, "0123"));
            Assert.AreEqual(PARSE_RESULT.ROOT_NOT_SINGULAR, JsonParser.Parse(v, "0x0"));
        }

        [TestMethod()]
        public void test_parse_number_too_big()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.NUMBER_TOO_BIG, JsonParser.Parse(v, "1e309"));
            Assert.AreEqual(PARSE_RESULT.NUMBER_TOO_BIG, JsonParser.Parse(v, "-1e309"));
        }

        [TestMethod()]
        public void test_parse_miss_comma_or_square_bracket()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.MISS_COMMA_OR_SQUARE_BRACKET, JsonParser.Parse(v, "[1"));
            Assert.AreEqual(PARSE_RESULT.MISS_COMMA_OR_SQUARE_BRACKET, JsonParser.Parse(v, "[1}"));
            Assert.AreEqual(PARSE_RESULT.MISS_COMMA_OR_SQUARE_BRACKET, JsonParser.Parse(v, "[1 2"));
            Assert.AreEqual(PARSE_RESULT.MISS_COMMA_OR_SQUARE_BRACKET, JsonParser.Parse(v, "[[]"));
        }

        [TestMethod()]
        public void test_parse_miss_key()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.MISS_KEY, JsonParser.Parse(v, "{:1,"));
            Assert.AreEqual(PARSE_RESULT.MISS_KEY, JsonParser.Parse(v, "{1:1,"));
            Assert.AreEqual(PARSE_RESULT.MISS_KEY, JsonParser.Parse(v, "{true:1,"));
            Assert.AreEqual(PARSE_RESULT.MISS_KEY, JsonParser.Parse(v, "{false:1,"));
            Assert.AreEqual(PARSE_RESULT.MISS_KEY, JsonParser.Parse(v, "{null:1,"));
            Assert.AreEqual(PARSE_RESULT.MISS_KEY, JsonParser.Parse(v, "{[]:1,"));
            Assert.AreEqual(PARSE_RESULT.MISS_KEY, JsonParser.Parse(v, "{{}:1,"));
            Assert.AreEqual(PARSE_RESULT.MISS_KEY, JsonParser.Parse(v, "{\"a\":1,"));
        }

        [TestMethod()]
        public void test_parse_miss_colon()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.MISS_COLON, JsonParser.Parse(v, "{\"a\"}"));
            Assert.AreEqual(PARSE_RESULT.MISS_COLON, JsonParser.Parse(v, "{\"a\",\"b\"}"));
        }

        [TestMethod()]
        public void test_parse_miss_comma_or_curly_bracket()
        {
            Value v = new Value();

            Assert.AreEqual(PARSE_RESULT.MISS_COMMA_OR_CURLY_BRACKET, JsonParser.Parse(v, "{\"a\":1"));
            Assert.AreEqual(PARSE_RESULT.MISS_COMMA_OR_CURLY_BRACKET, JsonParser.Parse(v, "{\"a\":1]"));
            Assert.AreEqual(PARSE_RESULT.MISS_COMMA_OR_CURLY_BRACKET, JsonParser.Parse(v, "{\"a\":1 \"b\""));
            Assert.AreEqual(PARSE_RESULT.MISS_COMMA_OR_CURLY_BRACKET, JsonParser.Parse(v, "{\"a\":{}"));
        }

        [TestMethod()]
        public void test_access_bool()
        {
            Value v = new Value();

            v.Bool = true;
            Assert.AreEqual(JSON_TYPE.TRUE, v.type);

            v.Bool = false;
            Assert.AreEqual(JSON_TYPE.FALSE, v.type);
        }

        [TestMethod()]
        public void test_access_number()
        {
            Value v = new Value();

            v.Num = 5566;
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(5566, v.Num);

            v.Num = -5566;
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(-5566, v.Num);

            v.Num = 55.66;
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(55.66, v.Num);

            v.Num = -55.66;
            Assert.AreEqual(JSON_TYPE.NUMBER, v.type);
            Assert.AreEqual(-55.66, v.Num);
        }

        [TestMethod()]
        public void test_access_string()
        {
            Value v;

            v = new Value();
            v.Str = "mingpepe";
            Assert.AreEqual(JSON_TYPE.STRING, v.type);
        }
    }
}