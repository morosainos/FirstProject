
continue https://confluence.kingland.com/display/DHTD/Request+to+Enrollment+Turnaround+Reduction
DEV run courier
https://confluence.kingland.com/display/TRAC/CourierRunBook

WebEx meeting   
https://kingland.webex.com/join/bekle   |  921 327 441     


http://10.6.48.40:8080/otpexternal/login.html


Bootstrap
http://www.cnblogs.com/landeanfen/p/4976838.html  bootstrap table
http://issues.wenzhixin.net.cn/bootstrap-table/
http://v2.bootcss.com/base-css.html   table css

  
<p class="bg-primary">...</p>
<p class="bg-success">...</p>
<p class="bg-info">...</p>
<p class="bg-warning">...</p>
<p class="bg-danger">...</p>  段落背景色

<input type="text" class="input-medium search-query"> 圆角输入框
<div class="input-append">
  <input class="span2" id="appendedInputButtons" type="text">
  <button class="btn" type="button">Search</button>
  <button class="btn" type="button">Options</button>
</div>  输入框和按钮连在一起


Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante.   2333

<!-- Favicon -->
<link rel="shortcut icon" href="img/favicon.ico" />


public class MD5Test {
 
    //main测试类
   public static void main(String[] args) {
        String result = getMD5("aaa");
        System.err.println(result);
    }
 
    /**
     * 生成md5
     * @param message
     * @return
     */
    public static String getMD5(String message) {
        String md5str = "";
        try {
            //1 创建一个提供信息摘要算法的对象，初始化为md5算法对象
            MessageDigest md = MessageDigest.getInstance("MD5");
 
            //2 将消息变成byte数组
            byte[] input = message.getBytes();
 
            //3 计算后获得字节数组,这就是那128位了
            byte[] buff = md.digest(input);
 
            //4 把数组每一字节（一个字节占八位）换成16进制连成md5字符串
            md5str = bytesToHex(buff);
 
        } catch (Exception e) {
            e.printStackTrace();
        }
        return md5str;
    }
 
    /**
     * 二进制转十六进制
     * @param bytes
     * @return
     */
    public static String bytesToHex(byte[] bytes) {
        StringBuffer md5str = new StringBuffer();
        //把数组每一字节换成16进制连成md5字符串
        int digital;
        for (int i = 0; i < bytes.length; i++) {
             digital = bytes[i];
 
            if(digital < 0) {
                digital += 256;
            }
            if(digital < 16){
                md5str.append("0");
            }
            md5str.append(Integer.toHexString(digital));
        }
        return md5str.toString().toUpperCase();
    }
}
