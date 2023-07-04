using DG.Tweening;
using Newtonsoft.Json;
using Spine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.U2D;
using UnityEngine.UI;

public static class Helpers
{
    private static HelperMono s_mono;
    private class HelperMono : MonoBehaviour
    {
        public async Task<Sprite> LoadSpriteAtlas(string atlasName, string spriteName, Action<Sprite> callback)
        {
            using (var op = AssetManager.LoadAssetAsync<SpriteAtlas>(atlasName))
            {
                await op;
                if (op == null || op.Result == null)
                {
                    callback(null);
                    return null;
                }

                if (callback != null)
                {
                    callback(op.Result.GetSprite(spriteName));
                }

                return op.Result.GetSprite(spriteName);
            }
        }

        public async void LoadSpriteAtlasWithoutDispose(string atlasName, Action<SpriteAtlas> callback)
        {
            var op = AssetManager.LoadAssetAsync<SpriteAtlas>(atlasName);
            {
                await op;
                if (op == null || op.Result == null)
                {
                    callback(null);
                    return;
                }
                callback(op.Result);
            }
        }

        public async Task<SpriteAtlas> LoadAtlas(string atlasName, Action<SpriteAtlas> callback)
        {
            using (var op = AssetManager.LoadAssetAsync<SpriteAtlas>(atlasName))
            {
                await op;
                if (op == null || op.Result == null)
                {
                    callback?.Invoke(null);
                    return null;
                }
                callback?.Invoke(op.Result);
                return op.Result;
            }
        }

    }

    static Helpers()
    {
        if (s_mono != null)
        {
            return;
        }
        var helperGo = new GameObject();
        helperGo.name = "HelperGO";
        helperGo.AddComponent<DontDestroyOnLoad>();
        s_mono = helperGo.AddComponent<HelperMono>();
        sCustomJsonSerializer.Converters.Add(new CustomDateJsonConverter());
    }

    #region 朝向旋转相关

    public static Quaternion LookAtSlerp(Vector3 vecLookDir, Quaternion curRotation, float slerpTime)
    {
        if (vecLookDir.sqrMagnitude <= 0.001f)
        {
            return curRotation;
        }

        return Quaternion.Slerp(curRotation, Quaternion.LookRotation(vecLookDir), slerpTime);
    }

    public static Vector2 RotateBy(Vector2 v, float a, bool bUseRadians = false)
    {
        if (!bUseRadians) a *= Mathf.Deg2Rad;
        var ca = System.Math.Cos(a);
        var sa = System.Math.Sin(a);
        var rx = v.x * ca - v.y * sa;

        return new Vector2((float)rx, (float)(v.x * sa + v.y * ca));
    }

    #endregion

    #region 距离相关

    /// <summary>
    /// 判断两点是否在距离范围内
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static bool IsWithinDistance(Vector3 source, Vector3 target, float radius)
    {
        if ((source - target).sqrMagnitude <= radius * radius)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static float GetDistanceSq(Vector3 from, Vector3 to)
    {
        return Vector3.SqrMagnitude(to - from);
    }

    public static float GetDistanceSqXZ(Vector3 from, Vector3 to)
    {
        from.y = 0;
        to.y = 0;
        return Vector3.SqrMagnitude(to - from);
    }

    /// <summary>
    /// 获取xy平面的距离
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static float GetDistanceXY(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(new Vector3(from.x, 0, from.z), new Vector3(to.x, 0, to.z));
    }

    public static bool IsReachPos(Vector3 pos, Vector3 targetPos, Vector3 Dir)
    {
        if (Dir.x >= 0)
        {
            // move right
            if (pos.x < targetPos.x)
            {
                return false;
            }
        }
        else
        {
            // move left
            if (pos.x > targetPos.x)
            {
                return false;
            }
        }

        if (Dir.y >= 0)
        {
            if (pos.y < targetPos.y)
            {
                return false;
            }
        }
        else
        {
            if (pos.y > targetPos.y)
            {
                return false;
            }
        }

        if (Dir.z >= 0)
        {
            if (pos.z < targetPos.z)
            {
                return false;
            }
        }
        else
        {
            if (pos.z > targetPos.z)
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsAgentReach(NavMeshAgent agent)
    {
        if (agent == null)
        {
            return false;
        }

        if (agent.pathPending == false 
            && agent.pathStatus == NavMeshPathStatus.PathComplete 
            && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (agent.velocity.sqrMagnitude == 0)
            {
                return true;
            }

            return false;
        }

        return false;
    }

    #endregion

    #region 脚本读取相关
    public static int[] SplitInt(string strContent, char separator = ',', bool excludeSingleZero = false)
    {
        if (strContent == null || strContent.Length <= 0)
        {
            return null;
        }

        var arrStr = strContent.Split(separator);
        if (arrStr == null)
        {
            return null;
        }
        else
        {
            int max = arrStr.Length;
            int[] arrResult = new int[max];
            for (int i = 0; i < max; ++i)
            {
                arrResult[i] = int.Parse(arrStr[i]);
            }

            if (excludeSingleZero == true && max == 1 && arrResult[0] == 0)
            {
                return null;
            }

            return arrResult;
        }
    }
    public static float[] SplitFloat(string strContent, char separator = ',')
    {
        if (strContent == null || strContent.Length <= 0)
        {
            return null;
        }

        var arrStr = strContent.Split(separator);
        if (arrStr == null)
        {
            return null;
        }
        else
        {
            int max = arrStr.Length;
            float[] arrResult = new float[max];
            for (int i = 0; i < max; ++i)
            {
                arrResult[i] = float.Parse(arrStr[i]);
            }

            return arrResult;
        }
    }

    public static double[] SplitDouble(string strContent, char separator = ',')
    {
        if (strContent == null || strContent.Length <= 0)
        {
            return null;
        }
        var arrStr = strContent.Split(separator);
        if (arrStr == null)
        {
            return null;
        }
        else
        {
            int max = arrStr.Length;
            double[] arrResult = new double[max];
            for (int i = 0; i < max; ++i)
            {
                arrResult[i] = double.Parse(arrStr[i]);
            }

            return arrResult;
        }
    }

    public static Vector3 ToVector3(string strContent, char separator = ',')
    {
        float[] arrFloat = SplitFloat(strContent, separator);
        if (arrFloat == null || arrFloat.Length < 3)
        {
            return new Vector3(0, 0, 0);
        }
        else
        {
            return new Vector3(arrFloat[0], arrFloat[1], arrFloat[2]);
        }
    }

    public static TValue GetWithinArray<TValue>(TValue[] array, int index)
    {
        if (array == null || array.Length == 0)
        {
            return default(TValue);
        }
        else
        {
            if (index <= 0)
            {
                return array[0];
            }
            else if (index > array.Length - 1)
            {
                return array[array.Length - 1];
            }
            else
            {
                return array[index];
            }
        }
    }

    public static int GetWithinArrayInt(int[] array, int index)
    {
        return GetWithinArray<int>(array, index);
    }

    public static float GetWithinArrayFloat(float[] array, int index)
    {
        return GetWithinArray<float>(array, index);
    }

    public static double GetWithinArrayDouble(double[] array, int index)
    {
        return GetWithinArray<double>(array, index);
    }

    public static TValue GetWithinArray<TValue>(List<TValue> array, int index)
    {
        if (array == null || array.Count == 0)
        {
            return default(TValue);
        }
        else
        {
            if (index <= 0)
            {
                return array[0];
            }
            else if (index > array.Count - 1)
            {
                return array[array.Count - 1];
            }
            else
            {
                return array[index];
            }
        }
    }


    public static TValue GetRanResultFromArray<TValue>(TValue[] cands, int[] pers) 
    {
        if (cands.Length != pers.Length) {
            return default(TValue);
        }

        int total = 0;
        int length = pers.Length;
        for (int i = 0; i < length; ++i) {
            total += pers[i];
        }

        int result = UnityEngine.Random.Range(0, total);
        for (int i = 0; i < length; ++i) {
            if (result <= pers[i]) {
                return cands[i];
            }
            else {
                result -= pers[i];
            }
        }

        return default(TValue);
    }

    public static int GetRandIntResultFromArray(int[] cands, int[] pers)
    {
        return GetRanResultFromArray<int>(cands, pers);
    }

    // 从数组[id1, per1, id2, per3, ...]中随机出一个id
    public static int GetRandIDFromIDRanList(int[] arrIDPerList, int startNum = 0) 
    {
        int totalNum = 0;
        for (int i = startNum, max = arrIDPerList.Length; i<max; i += 2)  {
            int id = arrIDPerList[i];
            totalNum += arrIDPerList[i + 1];
        }

        int ran = UnityEngine.Random.Range(0, totalNum);
        for (int i = startNum, max = arrIDPerList.Length; i<max; i += 2)  {
            int id = arrIDPerList[i];
            int per = arrIDPerList[i + 1];
            if (ran <= per)  {
                return id;
            }
            else  {
                ran -= per;
            }
        }

        return 0;
    }

    public static int GetRandResult(int[] arrRand)
    {
        int totalNum = 0;
        for (int i = 0, max = arrRand.Length; i < max; ++i)
        {
            totalNum += arrRand[i];
        }

        int ran = UnityEngine.Random.Range(0, totalNum);
        for (int i = 0, max = arrRand.Length; i < max; ++i)
        {
            int per = arrRand[i];
            if (ran <= per)
            {
                return i;
            }
            else
            {
                ran -= per;
            }
        }

        return 0;
    }

    private static System.Random mRand = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = mRand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    #endregion

    #region 控件操作相关
    public static T FindNamedComponentInChildren<T>(Transform trans, string name, bool includeInactive = true) where T : Component
    {
        var comps = trans.GetComponentsInChildren<T>(includeInactive);
        for (int i = 0, max = comps.Length; i < max; ++i)
        {
            var comp = comps[i];
            if (comp.gameObject.name == name || comp.gameObject.name == name + "(Clone)")
            {
                return comp;
            }
        }

        return null;
    }

    public static ParticleSystemRenderer[] FindChildrenPsrComponentInTrans(Transform trans)
    {
        var comps = trans.GetComponentsInChildren<ParticleSystemRenderer>(true);
        return comps;
    }

    /// <summary>
    /// 先尝试获取指定的Component，获取不了则添加
    /// </summary>
    /// <param name="go"></param>
    public static T AddComponentIfNotExist<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
        }

        return comp;
    }

    public static Component MoveComponentToAnotherGameObject(Component comp, GameObject go, bool deleteFromComp = true)
    {
        if (comp is NavMeshAgent)
        {
            var agent = comp as NavMeshAgent;
            var newAgent = go.AddComponent<NavMeshAgent>();
            newAgent.speed = agent.speed;
            newAgent.updateRotation = agent.updateRotation;
            newAgent.updateUpAxis = agent.updateUpAxis;
            newAgent.stoppingDistance = agent.stoppingDistance;
            newAgent.baseOffset = agent.baseOffset;
            newAgent.radius = agent.radius;
            newAgent.height = agent.height;
            newAgent.acceleration = agent.acceleration;
            newAgent.autoBraking = agent.autoBraking;
            newAgent.autoRepath = agent.autoRepath;
            newAgent.obstacleAvoidanceType = agent.obstacleAvoidanceType;
            newAgent.avoidancePriority = agent.avoidancePriority;
            newAgent.autoTraverseOffMeshLink = agent.autoTraverseOffMeshLink;
            newAgent.enabled = agent.enabled;

            if (deleteFromComp == true)
            {
                GameObject.Destroy(comp);
            }
            return newAgent;
        }
        else if (comp is NavMeshObstacle)
        {
            var agent = comp as NavMeshObstacle;
            var newAgent = go.AddComponent<NavMeshObstacle>();
            newAgent.shape = agent.shape;
            newAgent.center = agent.center;
            newAgent.size = agent.size;
            newAgent.radius = agent.radius;
            newAgent.height = agent.height;
            newAgent.enabled = agent.enabled;

            if (deleteFromComp == true)
            {
                GameObject.Destroy(comp);
            }

            return newAgent;
        }

        return null;
    }

    #endregion

    #region 加密解密

    public static string IV = "1a1a1a1a1a1a1a1a";
    public static string Key = "1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a13";

    public static string Encrypt(string decrypted)
    {
        byte[] textbytes = ASCIIEncoding.ASCII.GetBytes(decrypted);
        AesCryptoServiceProvider endec = new AesCryptoServiceProvider();
        endec.BlockSize = 128;
        endec.KeySize = 256;
        endec.IV = ASCIIEncoding.ASCII.GetBytes(IV);
        endec.Key = ASCIIEncoding.ASCII.GetBytes(Key);
        endec.Padding = PaddingMode.PKCS7;
        endec.Mode = CipherMode.CBC;
        ICryptoTransform icrypt = endec.CreateEncryptor(endec.Key, endec.IV);
        byte[] enc = icrypt.TransformFinalBlock(textbytes, 0, textbytes.Length);
        icrypt.Dispose();
        return Convert.ToBase64String(enc);
    }

    public static string Decrypted(string encrypted)
    {
        byte[] textbytes = Convert.FromBase64String(encrypted);
        AesCryptoServiceProvider endec = new AesCryptoServiceProvider();
        endec.BlockSize = 128;
        endec.KeySize = 256;
        endec.IV = ASCIIEncoding.ASCII.GetBytes(IV);
        endec.Key = ASCIIEncoding.ASCII.GetBytes(Key);
        endec.Padding = PaddingMode.PKCS7;
        endec.Mode = CipherMode.CBC;
        ICryptoTransform icrypt = endec.CreateDecryptor(endec.Key, endec.IV);
        byte[] enc = icrypt.TransformFinalBlock(textbytes, 0, textbytes.Length);
        icrypt.Dispose();
        return System.Text.ASCIIEncoding.ASCII.GetString(enc);
    }

    public static byte[] Encrypt(byte[] input)
    {
        PasswordDeriveBytes pdb =
          new PasswordDeriveBytes("hjiweykaksd", // Change this
          new byte[] { 0x43, 0x87, 0x23, 0x72 }); // Change this
        MemoryStream ms = new MemoryStream();
        Aes aes = new AesManaged();
        aes.Key = pdb.GetBytes(aes.KeySize / 8);
        aes.IV = pdb.GetBytes(aes.BlockSize / 8);
        CryptoStream cs = new CryptoStream(ms,
          aes.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(input, 0, input.Length);
        cs.Close();
        return ms.ToArray();
    }
    public static byte[] Decrypt(byte[] input)
    {
        PasswordDeriveBytes pdb =
          new PasswordDeriveBytes("hjiweykaksd", // Change this
          new byte[] { 0x43, 0x87, 0x23, 0x72 }); // Change this
        MemoryStream ms = new MemoryStream();
        Aes aes = new AesManaged();
        aes.Key = pdb.GetBytes(aes.KeySize / 8);
        aes.IV = pdb.GetBytes(aes.BlockSize / 8);
        CryptoStream cs = new CryptoStream(ms,
          aes.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(input, 0, input.Length);
        cs.Close();
        return ms.ToArray();
    }

    #endregion

    #region Writer and Reader
    public static async Task WriteFileAsync(string path, string content)
    {
        using (StreamWriter outputFile = new StreamWriter(path))
        {
            await outputFile.WriteAsync(content);
        }
    }
    public static async Task WriteBytesAsync(string path, byte[] content)
    {
        using (FileStream outputFile = File.Open(path, FileMode.OpenOrCreate))
        {
            await outputFile.WriteAsync(content, 0, content.Length);
        }
    }

    public static async Task<byte[]> ReadBytesAsync(string path)
    {
        using (FileStream SourceStream = File.Open(path, FileMode.Open))
        {
            var result = new byte[SourceStream.Length];
            await SourceStream.ReadAsync(result, 0, (int)SourceStream.Length);

            return result;
        }
    }
    #endregion

    #region 容器扩展相关
    public static TValue Find<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key)
    {
        TValue value;
        source.TryGetValue(key, out value);
        return value;
    }

    public static string GetDicIntStr(Dictionary<int, string> dic, int key)
    {
        string value;
        dic.TryGetValue(key, out value);
        return value;
    }

    public static string GetDicStrStr(Dictionary<string, string> dic, string key)
    {
        string value;
        dic.TryGetValue(key, out value);
        return value;
    }
    public static List<int> GetDicIntList(Dictionary<int, List<int>> dic, int key)
    {
        List<int> value = null;
        dic.TryGetValue(key, out value);
        return value;
    }

    #endregion

    #region JsonConverter
    public class CustomDateJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset))
            {
                return true;
            }

            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DateTimeOffset dateTimeOff = DateTimeOffset.FromUnixTimeMilliseconds((long)reader.Value);
            if (objectType == typeof(DateTime))
            {
                return dateTimeOff.LocalDateTime;
            }
            return dateTimeOff.ToLocalTime();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long time = 0;
            if (value.GetType() == typeof(DateTime))
            {
                time = new DateTimeOffset(((DateTime)value)).ToUnixTimeMilliseconds();
            }
            else
            {
                time = ((DateTimeOffset)value).ToUnixTimeMilliseconds();
            }

            writer.WriteValue(time);
        }
    }

    public static JsonConverter[] sCustomJsonConverter = { new CustomDateJsonConverter() };
    public static JsonSerializer sCustomJsonSerializer = JsonSerializer.CreateDefault();
    #endregion

    #region 杂类相关

    private static Dictionary<int, object> sContextDic = new Dictionary<int, object>();
    public static Dictionary<int, object> GetStatusContext()
    {
        sContextDic.Clear();
        return sContextDic;
    }

    public static void TryParseString(JSONObject json, string key, out string value, string defaultValue = "")
    {
        JSONObject jsonTemp = json[key];
        if (jsonTemp != null)
        {
            value = jsonTemp.str;
            return;
        }

        value = defaultValue;
    }

    public static void TryParseBool(JSONObject json, string key, out bool value, bool defaultValue = false)
    {
        JSONObject jsonTemp = json[key];
        if (jsonTemp != null)
        {
            if (Boolean.TryParse(jsonTemp.str, out value) == true)
            {
                return;
            }
        }

        value = defaultValue;
    }
    public static void TryParseInt(JSONObject json, string key, out int value, int defaultValue = 0)
    {
        JSONObject jsonTemp = json[key];
        if (jsonTemp != null)
        {
            if (int.TryParse(jsonTemp.str, out value) == true)
            {
                return;
            }
        }

        value = defaultValue;
    }
    public static void TryParseFloat(JSONObject json, string key, out float value, float defaultValue = 0)
    {
        JSONObject jsonTemp = json[key];
        if (jsonTemp != null)
        {
            if (float.TryParse(jsonTemp.str, out value) == true)
            {
                return;
            }
        }

        value = defaultValue;
    }
    public static void TryParseDouble(JSONObject json, string key, out double value, double defaultValue = 0)
    {
        JSONObject jsonTemp = json[key];
        if (jsonTemp != null)
        {
            if (double.TryParse(jsonTemp.str, out value) == true)
            {
                return;
            }
        }

        value = defaultValue;
    }

    public static bool TryParseVector3(JSONObject json, string key, out Vector3 value, Vector3 defaultValue = default(Vector3))
    {
        JSONObject jsonTemp = json[key];
        if (jsonTemp != null)
        {
            var strContent = jsonTemp.str;
            float[] arrFloat = SplitFloat(strContent, ',');
            if (arrFloat == null || arrFloat.Length < 3)
            {
                value = defaultValue;
                return false;
            }
            else
            {
                value = new Vector3(arrFloat[0], arrFloat[1], arrFloat[2]);
                return true;
            }
        }

        value = defaultValue;
        return false;
    }

    public static void TryParseFloats(JSONObject json, string key, out float[] value)
    {
        JSONObject jsonTemp = json[key];
        if (jsonTemp != null)
        {
            value = Helpers.SplitFloat(jsonTemp.str);
            return;
        }
        
        value = null;
    }

    public static void TryParseInts(JSONObject json, string key, out int[] value)
    {
        JSONObject jsonTemp = json[key];
        if (jsonTemp != null)
        {
            value = Helpers.SplitInt(jsonTemp.str);
            return;
        }

        value = null;
    }

    public static void TryParseStrings(JSONObject json, string key, out string[] value)
    {
        JSONObject jsonTemp = json[key];
        if (jsonTemp != null && string.IsNullOrEmpty(jsonTemp.str) == false)
        {
            value = jsonTemp.str.Split(',');
            return;
        }

        value = null;
    }


    public static bool GetBit(int value, int index)
    {
        if ((value & (1<<index)) != 0){
            return true;
        }

        return false;
    }
    public static int SetBit(int value, int index, bool set)
    {
        if (set)
        {
            return (value | (1 << index));
        }
        else
        {
            return (value & (~(1 << index)));
        }
    }

    public static UnityWebRequest Post(string url, string param)
    {
        var www = new UnityWebRequest(url, "POST");

        if (param.Length > 0)
        {
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(param));
        }
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        return www;
    }

    public static UnityWebRequest PostForm(string url, WWWForm form)
    {
        form.AddField("Content-Type", "application/x-www-form-urlencoded");
        var www = UnityWebRequest.Post(url, form);
        return www;
    }

    public static async Task<string> PostJson(string url, string strJson)
    {
        var www = new UnityWebRequest(url, "POST");
        if (strJson.Length > 0)
        {
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(strJson));
        }
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        using (www)
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError 
                || www.result == UnityWebRequest.Result.ProtocolError)
            {
                return null;
            }

            return www.downloadHandler.text;
        }
    }

    public static async Task<Texture2D> GetTextureFromUrl(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        using (www)
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                return null;
            }

            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            return texture;
        }
    }

    public static async void SetSpriteRenderFromURL(string url, SpriteRenderer spriteRender, float designSize = 0)
    {
        var texture = await GetTextureFromUrl(url);
        if (texture == null)
        {
            return;
        }

        if (spriteRender == null)
        {
            return;
        }

        Rect rec = new Rect(0, 0, texture.width, texture.height);
        Sprite sprite = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), 100.0f);
        spriteRender.sprite = sprite;

        if (designSize != 0)
        {
            float scale = designSize / texture.width;
            spriteRender.transform.localScale = Vector3.one * scale;
        }
    }

    public static async void SetImageFromURL(string url, Image img)
    {
        var texture = await GetTextureFromUrl(url);
        if (texture == null)
        {
            return;
        }

        if (img == null)
        {
            return;
        }

        Rect rec = new Rect(0, 0, texture.width, texture.height);
        Sprite sprite = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), 100.0f);
        img.sprite = sprite;
    }
   
    private static string s_RandKey = "abcdefghijklmnopqrsduvwxyz1234567890";
    private static int s_RandKeyLength = s_RandKey.Length;

    public static string RandString(int length)
    {
        // 随机字符串
        string ret = "";
        for (int i = 0; i< length; ++i) 
        {
            float ran = UnityEngine.Random.Range(0.0f, 1.0f) * s_RandKeyLength;
            int index = (int)Mathf.Floor(ran);
            ret += s_RandKey[index];
        }

        return ret;
    }

    private static string []sBigNumUnit = new string[] { "K", "M", "B", "T", "P", "E", "Z", "Y", "N", "D", "a", "b", "c", "d", "e" };
    public static string GetBigNumberString(double num) 
    {
        if (num< 1000) {
            return num.ToString("N0");
        }

        double time = 1000000.0d;
        int index = 0;
        string strTemp = "";
        do {
            if (num >= time) {
                time *= 1000.0d;
                index++;
                continue;
            }
            double fTemp = num / (time / 1000.0d);
            if (fTemp< 10) {
                strTemp = double.Parse(fTemp.ToString("N2")) + "";
            }
            else if (fTemp< 100) {
                strTemp = double.Parse(fTemp.ToString("N1")) + "";
            }
            else {
                strTemp = fTemp.ToString("N0");
            }
            break;
        } while (true);
        if (index >= sBigNumUnit.Length) {
            return "000";
        }

        return strTemp + sBigNumUnit[index];
    }

    public static string StringResFormat(string id, params object[] args)
    {
        var stringFormat = DataManager.Instance.GetStringRes(id);
        return string.Format(stringFormat, args);
    }

    public static void SetLayerToAll(Transform trans, int layer)
    {
        foreach (Transform curTran in trans.GetComponentsInChildren<Transform>())
        {
            curTran.gameObject.layer = layer;
        }
    }

    public static void AddDragEvent(EventTrigger trigger, Action<PointerEventData> callback)
    {
         EventTrigger.Entry entry = new EventTrigger.Entry();
         entry.eventID = EventTriggerType.Drag;
         entry.callback.AddListener((data) => { callback((PointerEventData)data); });
         trigger.triggers.Add(entry);
    }

    public static void AddTriggerEvent(EventTrigger trigger, int triggerType, Action<PointerEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = (EventTriggerType)triggerType;
        entry.callback.AddListener((data) => { callback((PointerEventData)data); });
        trigger.triggers.Add(entry);
    }

    public static void AddScrollRectTriggerEvent(ScrollRect scrollRect)
    {
    }

    public static void RemoveDragEvent(EventTrigger trigger)
    {
        trigger.triggers.Clear();
    }

    public static string GetMinuteBySecond(int second)
    {
        int day = Mathf.FloorToInt(second / 86400);
        int hour = Mathf.FloorToInt(second / 3600) % 24;
        int minute = Mathf.FloorToInt(second / 60) % 60;
        int curSecond = second % 60;
        string HourStr = hour.ToString();
        string MinuteStr = minute.ToString();
        string SecondStr = curSecond.ToString();
        if(minute < 10)
        {
            MinuteStr = "0" + MinuteStr;
        }
        if(curSecond < 10)
        {
            SecondStr = "0" + SecondStr;
        }
        if(hour < 10)
        {
            HourStr = "0" + HourStr;
        }

        if(hour <= 0)
        {
            return  minute + ":" + SecondStr;
        }
        if(day <= 0)
        {
            return hour + ":" + MinuteStr + ":" + SecondStr;
        }

        return day + DataManager.Instance.GetStringRes("Day") + HourStr + ":" + MinuteStr + ":" + SecondStr;
    }

    public static string GetLastTimeBySecond(int second)
    {
        int day = Mathf.FloorToInt(second / 86400);
        int hour = Mathf.FloorToInt(second / 3600) % 24;
        int minute = Mathf.FloorToInt(second / 60) % 60;
        int curSecond = second % 60;
        string HourStr = hour.ToString();
        string MinuteStr = minute.ToString();
        string SecondStr = curSecond.ToString();

        if(day <= 0)
        {
            if(hour <= 0)
            {
                if(minute <= 0)
                {
                    return 1 + DataManager.Instance.GetStringRes("Minute") + " " + DataManager.Instance.GetStringRes("Ago");
                }
                else
                {
                    return minute + DataManager.Instance.GetStringRes("Minute") + " " + DataManager.Instance.GetStringRes("Ago");
                }
            }
            else
            {
                return hour + DataManager.Instance.GetStringRes("Hour") + " " + DataManager.Instance.GetStringRes("Ago");
            }
        }
        else if(day < 7)
        {
            return day + DataManager.Instance.GetStringRes("Day") + " " + DataManager.Instance.GetStringRes("Ago");
        }
        else
        {
            return 7 + DataManager.Instance.GetStringRes("Day") + " " + DataManager.Instance.GetStringRes("Ago");
        }
    }

    // 获取当前时间戳
    public static long GetTimeStamp()
    {
        TimeSpan timeStamp = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(timeStamp.TotalSeconds);
    }


    public static bool CheckLineSphereIntersection(Vector3 p1, Vector3 p2, Vector3 center, float radius)
    {
        Vector3 dp = new Vector3();
        float a, b, c;
        float bb4ac;
        dp.x = p2.x - p1.x;
        dp.z = p2.z - p1.z;
        a = dp.x * dp.x + dp.z * dp.z;
        b = 2 * (dp.x * (p1.x - center.x) + dp.z * (p1.z - center.z));
        c = center.x * center.x + center.z * center.z;
        c += p1.x * p1.x + p1.z * p1.z;
        c -= 2 * (center.x * p1.x + center.z * p1.z);
        c -= radius * radius;
        bb4ac = b * b - 4 * a * c;
        if (Mathf.Abs(a) < float.Epsilon || bb4ac < 0)
        {
            return false;
        }

        return true;
    }

    public static bool CheckLineCircleIntersection(float cx, float cy, float radius, 
        float p1x, float p1y, float p2x, float p2y)
    {
        float dx, dy, A, B, C, det;

        dx = p2x - p1x;
        dy = p2y - p1y;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (p1x - cx) + dy * (p1y - cy));
        C = (p1x - cx) * (p1x - cx) +
            (p1y - cy) * (p1y - cy) -
            radius * radius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static void ResetParticlesAndLines(Transform trans)
    {
        var particles = trans.GetComponentsInChildren<ParticleSystem>();
        foreach (var bulletParticle in particles)
        {
            bulletParticle.Clear();
            bulletParticle.Simulate(0.0f, true, true);
            bulletParticle.Play();
        }
        var trails = trans.GetComponentsInChildren<TrailRenderer>();
        foreach (var trail in trails)
        {
            trail.Clear();
        }
    } 
    
    #endregion

    #region 资源加载相关

    /// <summary>
    /// 从载入图集并获取图片
    /// </summary>
    /// <param name="atlasName"></param>
    /// <param name="spriteName"></param>
    /// <param name="callback"></param>
    public static async Task<Sprite> LoadSpriteAtlas(string atlasName, string spriteName, Action<Sprite> callback)
    {
        return await s_mono.LoadSpriteAtlas(atlasName, spriteName, callback);
    }

    public static void LoadSpriteAtlasWithoutDispose(string packageName, string atlasName, Action<SpriteAtlas> callback)
    {
        s_mono.LoadSpriteAtlasWithoutDispose(atlasName, callback);
    }

    public static async Task<SpriteAtlas> LoadAtlas(string atlasName, Action<SpriteAtlas> callback)
    {
        return await s_mono.LoadAtlas(atlasName, callback);
    }

    /// <summary>
    /// 随机一个整数
    /// </summary>
    /// <param name="startInclude"></param>
    /// <param name="endExclusive"></param>
    /// <returns></returns>
    public static int RandInt(int startInclude, int endExclusive)
    {
        return UnityEngine.Random.Range(startInclude, endExclusive);
    }

    /// <summary>
    /// 随机一个boolean值
    /// </summary>
    /// <param name="truePer"></param>
    /// <param name="falsePer"></param>
    /// <returns></returns>
    public static bool RandBool(int truePer = 10000, int falsePer = 10000)
    {
        int totalPer = truePer + falsePer;
        int per = UnityEngine.Random.Range(0, totalPer);
        if (per <= truePer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static List<int> CreateList(params int[] args)
    {
        List<int> result = new List<int>();
        foreach (var value in args)
        {
            result.Add(value);
        }
        return result;
    }

    #endregion

    #region 测试工具相关

    #endregion

    #region 坐标转换
    public static Vector3 GetMousePositionInRectTrans(RectTransform canvasRectTransform, Camera camera)
    {
        Vector3 result = Vector3.zero;
        Vector2 tempVector = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, Input.mousePosition, camera, out tempVector);
        //result = canvasRectTransform.TransformPoint(tempVector);
        return tempVector;
    }

    public static Vector2 WorldPositionUIAnchorPos(Vector3 worldPos, bool clampInsideScreen = false, float minX = 0, float maxX = 1, float minY = 0, float maxY = 1)
    {
        RectTransform targetCanvas = UIManager.Instance.CurCanvas.GetComponent<RectTransform>();
        Camera uiCamera = UIManager.Instance.UICamera;
        Camera worldCamera = Camera.main;
        if (worldCamera == null || worldCamera.gameObject.activeSelf == false)
        {
            return Vector2.zero;
        }
        Vector2 viewportPosition = worldCamera.WorldToViewportPoint(worldPos);
        if (clampInsideScreen == true)
        {
            viewportPosition.x = Mathf.Clamp(viewportPosition.x, minX, maxX);
            viewportPosition.y = Mathf.Clamp(viewportPosition.y, minY, maxY);
        }
        Vector2 screenPos = new Vector2(
        ((viewportPosition.x * targetCanvas.sizeDelta.x) - (targetCanvas.sizeDelta.x * 0.5f)),
        ((viewportPosition.y * targetCanvas.sizeDelta.y) - (targetCanvas.sizeDelta.y * 0.5f)));
        return screenPos;
    }
    //public static Vector2 GetRectTransByMousePos()
    //{
    //    Vector2 pos = Vector2.zero;
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(UIManager.Instance.CurCanvasRectTrans, Input.mousePosition, UIManager.Instance.UICamera, out pos);
    //    return pos;
    //}

    //public static Vector2 GetWordlRectTransByUIPostition(Vector3 targetPos)
    //{
    //    Vector2 pos = Vector2.zero;
    //    Debug.Log(targetPos);
    //    pos = UIManager.Instance.UICamera.WorldToScreenPoint(targetPos);
    //    pos = RectTransformUtility.WorldToScreenPoint(UIManager.Instance.UICamera,targetPos);
    //    Debug.Log(pos);
    //    return pos;
    //}

    public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
    {
        Vector2 localPoint;
        Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
        Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
        screenP += fromPivotDerivedOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
        Vector2 pivotDerivedOffset = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin, to.rect.height * to.pivot.y + to.rect.yMin);
        return to.anchoredPosition + localPoint - pivotDerivedOffset;
    }

    public static void SetSortingOrder(Canvas canvas, int order)
    {
        canvas.overrideSorting = true;
        canvas.sortingLayerID = SortingLayer.NameToID("UI");
        canvas.sortingOrder = order;
        var rect = canvas.transform.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(0,0);
    }

    public static string MD5Encrypt32(this string str)
    {
        using (var cryptoMD5 = System.Security.Cryptography.MD5.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            var hash = cryptoMD5.ComputeHash(bytes);
            var md5 = BitConverter.ToString(hash)
              .Replace("-", String.Empty)
              .ToLower();

            return md5;
        }
    }

    // 获取sign
    public static string GetSign(params string []values)
    {
        string secret = "98ec8ef0dd49ad4666da52fd39e2d085";
        string plain = "";
        
        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                plain += value;
            }
        }

        string sign = MD5Encrypt32(plain + secret);
        return sign;
    }

    public static string GetSign(List<KeyValuePair<string, string>> keyvalues)
    {
        keyvalues.Sort(
            (KeyValuePair<string, string> a1,
            KeyValuePair<string, string> a2) =>
            {
                return string.Compare(a1.Key, a2.Key);
            }
            );

        string secret = "98ec8ef0dd49ad4666da52fd39e2d085";
        string plain = "";

        foreach (var kv in keyvalues)
        {
            if (string.IsNullOrEmpty(kv.Value) == false)
            {
                plain += kv.Value;
            }
        }

        string sign = MD5Encrypt32(plain + secret);
        return sign;
    }

    #endregion

    #region 对象创建相关
    //public async static void InstantiateLua(string assetbundle, string assetName, Action<GameObject> callback)
    //{
    //    using (var op = BundleManager.LoadAsync<GameObject>(assetbundle, assetName))
    //    {
    //        await op;
    //        var go = BundleManager.Instantiate(op.Asset);
    //        callback?.Invoke(go);
    //    }
    //}
    #endregion

    #region 设备相关
    public static readonly float FringeFitTop = 72;
    public static readonly float FringeFitBottom = 0;
    public static int ScreenWidth = 0;
    public static int ScreenHeight = 0;

    public static bool IsFringeFit() 
    {
        if (Screen.height / Screen.width < 2)
        {
            return false;
        }

        return true;
    }

    public static bool IsWidthFit()
    {
        if (Screen.width / (float)Screen.height > 720 / 1280.0f)
        {
            return true;
        }
        return false;
    }

    public static float GetWidthFitPer()
    {
        float minWidthHeightRatio = 0.5625f;
        float targetIpadHeight = Screen.width / minWidthHeightRatio;
        float per = (targetIpadHeight - Screen.height) / targetIpadHeight;
        return per;
    }

    public static int GetScreenWidth()
    {
        return Screen.width;
    }
    public static int GetScreenHeight()
    {
        return Screen.height;
    }
    #endregion

    #region Tween相关
    #endregion

    #region UI相关
    public static void TextOutline(TextMeshProUGUI text, float width, Color color, float dilate)
    {
        text.extraPadding = true;
        text.ForceMeshUpdate(true, true);
        var mats = text.fontMaterials;
        foreach (var m in mats)
        {
            m.SetColor(ShaderUtilities.ID_OutlineColor, color);
            m.SetFloat(ShaderUtilities.ID_OutlineWidth, width);
            m.SetFloat(ShaderUtilities.ID_FaceDilate, dilate);
        }
    }

    public static void TextOutline3D(TextMeshPro text, float width, Color color, float dilate)
    {
        text.extraPadding = true;
        text.ForceMeshUpdate(true, true);
        var mats = text.fontMaterials;
        foreach (var m in mats)
        {
            m.SetColor(ShaderUtilities.ID_OutlineColor, color);
            m.SetFloat(ShaderUtilities.ID_OutlineWidth, width);
            m.SetFloat(ShaderUtilities.ID_FaceDilate, dilate);
        }
    }

    public static void SetMaterialColor(Transform trans, Color color)
    {
        var renders = trans.GetComponentsInChildren<Renderer>();
        foreach(var r in renders)
        {
            var mats = r.materials;
            foreach(var m in mats)
            {
                string shaderName = m.shader.name;
                if (shaderName == "tinyhorse/toon" || shaderName == "tinyhorse/toonTransparent")
                {
                    m.SetColor("_MainColor", color);
                }
            }
        }
    }

    public static void UpdateLocaleText(TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        var curFontAsset = LocaleManager.Instance.GetMainFontAsset(LocaleManager.Instance.curLocale);
        if (curFontAsset == null)
        {
            return;
        }
        
        text.font = curFontAsset;
        text.ForceMeshUpdate(true, true);
    }

    public static void UpdateLocale3DText(TextMeshPro text)
    {
        if (text == null)
        {
            return;
        }

        var curFontAsset = LocaleManager.Instance.GetMainFontAsset(LocaleManager.Instance.curLocale);
        if (curFontAsset == null)
        {
            return;
        }

        text.font = curFontAsset;
        text.ForceMeshUpdate(true, true);
    }

    //public static void FlyMstIcon(int num, Vector3 from, Vector3 to)
    //{
    //    for (int i = 0; i < num; ++i)
    //    {
    //        var icon = ViewManager.Instance.CreateView("FlyMstItem");
    //        var layer = UIManager.Instance.GetLayer(EnCanvasLayer.top);
    //        icon.trans.SetParent(layer.transform, false);
    //        var seq = DOTween.Sequence();
    //        icon.trans.position = from;

    //        Vector3 point1 = layer.transform.InverseTransformPoint(from) + new Vector3(UnityEngine.Random.Range(-80, 80), UnityEngine.Random.Range(-50, 50), 0);
    //        Vector3 point2 = layer.transform.InverseTransformPoint(to) + new Vector3(UnityEngine.Random.Range(-30, 30), UnityEngine.Random.Range(-30, 30), 0);

    //        if(i == 0)
    //        {
    //            seq.Append(icon.trans.DOLocalMove(point1, 0.5f).SetEase(Ease.OutExpo));
    //            seq.Append(icon.trans.DOLocalMove(point2, 0.5f).SetEase(Ease.InBack));
    //        }
    //        else if(i == num -1)
    //        {
    //            seq.Append(icon.trans.DOLocalMove(point1, 1).SetEase(Ease.OutExpo));
    //            seq.Append(icon.trans.DOLocalMove(point2, 1).SetEase(Ease.InBack));
    //        }
    //        else
    //        {
    //            seq.Append(icon.trans.DOLocalMove(point1, UnityEngine.Random.Range(0.5f, 1.0f)).SetEase(Ease.OutExpo));
    //            seq.Append(icon.trans.DOLocalMove(point2, UnityEngine.Random.Range(0.5f, 1.0f)).SetEase(Ease.InBack));
    //        }


    //        seq.onComplete = () => {
    //            icon.Release(false);
    //        };

    //    }
    //}

    // -1 undefined, 0 no show text, 1 show text
    private static int mCanFloatDamageText = -1;
    private static string mFloatDamageTextKey = "floatDamageText";
    public static bool CanFloatDamageText()
    {
        if (mCanFloatDamageText == -1)
        {
            InitFloatDamageText();
        }

        if (mCanFloatDamageText > 0)
        {
            return true;
        }
        return false;
    }
    public static void SetFloatDamageText(bool show)
    {
        if (show == true)
        {
            PlayerPrefs.SetInt(mFloatDamageTextKey, 1);
            mCanFloatDamageText = 1;
        }
        else
        {
            PlayerPrefs.SetInt(mFloatDamageTextKey, 0);
            mCanFloatDamageText = 0;
        }
    }
    private static void InitFloatDamageText()
    {
        if (PlayerPrefs.HasKey(mFloatDamageTextKey) == true)
        {
            mCanFloatDamageText = PlayerPrefs.GetInt(mFloatDamageTextKey);
        }
        else
        {
            // default to show
            mCanFloatDamageText = 1;
            PlayerPrefs.SetInt(mFloatDamageTextKey, mCanFloatDamageText);
        }
    }

    private static int mCanVibrate = -1;
    private static string mVibrateKey = "vibrate";
    public static bool CanVibrate()
    {
        if (mCanVibrate == -1)
        {
            InitVibrate();
        }

        if (mCanVibrate > 0)
        {
            return true;
        }
        return false;
    }
    public static void SetVibrate(bool show)
    {
        if (show == true)
        {
            PlayerPrefs.SetInt(mVibrateKey, 1);
            mCanVibrate = 1;
        }
        else
        {
            PlayerPrefs.SetInt(mVibrateKey, 0);
            mCanVibrate = 0;
        }
    }
    private static void InitVibrate()
    {
        if (PlayerPrefs.HasKey(mVibrateKey) == true)
        {
            mCanVibrate = PlayerPrefs.GetInt(mVibrateKey);
        }
        else
        {
            // default to show
            mCanVibrate = 1;
            PlayerPrefs.SetInt(mVibrateKey, mCanVibrate);
        }
    }
#endregion

#region 类操作相关
    public static string GetStaticFieldValueVariableName(object checkValue, Type type)
    {
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            var value = field.GetValue(null);

            Debug.Log("state: " + field.Name + " value:" + value);

            if (value == checkValue)
            {
                return field.Name;
            }
        }
        return checkValue.ToString();
    }
#endregion

#region 音效相关
    public static bool isPlayingInterludeAnimationMusic = false;
    #endregion

    #region 生物属性相关

    /// <summary>
    /// 根据当前所需攻击时长, 获取攻击动画的播放速度
    /// </summary>
    /// <param name="baseTime">配置里的1次攻击得时间</param>
    /// <param name="animTime">攻击动画的播放时长</param>
    /// <param name="curAttackTime">当前所需攻击动作的总时长</param>
    /// <returns></returns>
    public static float GetAniSpeedByAttackSpeed(float baseTime, float animTime, float curAttackTime)
    {
        float aniFinalSpeed = 1.0f;
        if (animTime >= baseTime)
        {
            // 配置的攻击速度比动画的速度还要快, 则加快动画的播放速度
            float aniSpeedByCfg = animTime / baseTime;
            aniFinalSpeed = aniSpeedByCfg;
        }

        aniFinalSpeed *= baseTime / curAttackTime;
        return aniFinalSpeed;
    }

#endregion
}

