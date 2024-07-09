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
	base64Str  = "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"
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
