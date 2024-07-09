package main

import (
	"C"
	"encoding/json"
	"net/url"

	"github.com/volcengine/volc-sdk-golang/service/visual"
)

const (
	kAccessKey = "xx" // https://console.volcengine.com/iam/keymanage/
	kSecretKey = "xx"
	base64Str  = ""
)

func main() {
	//Execute(kAccessKey, kSecretKey, base64Str)
}

// export Execute
func Execute(accessKey, secretKey, base64Str *C.char) (int, string) {
	visual.DefaultInstance.Client.SetAccessKey(C.GoString(accessKey))
	visual.DefaultInstance.Client.SetSecretKey(C.GoString(secretKey))
	form := url.Values{}
	form.Add("image_base64", C.GoString(base64Str))
	resp, status, err := visual.DefaultInstance.OCRNormal(form)
	b, _ := json.Marshal(resp)
	if status == 200 {
		return status, string(b)
	} else if err == nil {
		return status, string(b)
	} else {
		return status, err.Error()
	}
}
