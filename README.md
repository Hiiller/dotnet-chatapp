# dotnet-chatapp
A chatapp based on dotnet using c#.
业务api实现：
前端：WelcomeView{
Localhost-->http;

LoginResponse{
    Guid id
    bool status
}

LoginUser ( Dto ) -> return LoginResponse;
RegisterUser ( Dto ) -> return LoginResponse;


}---   LoginResponse,_httpClient    --->ChatListView
ChatList
{
  new=> _connection
  getRecentMessage (LoginResponse){
     var content = new StringContent(JsonSerializer.Serialize(loginDto),Encoding.UTF8,"application/json");
            var response = await _httpClient.PostAsync("/api/chat/getReceiverList",content);
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync< List[Guid] >(responseStream);
                Display(result);
                return ;
            }
  }
     Display(Guid List){
     ....
     }
     Transfer(  Guid id  ){
        Excute( new ChatView(id, Router))
     }

  
}--  ReceiverId -> ChatView
{
    hubservice->ConnectAsync(  RId ) ---连接到signalR
    => _connection
    send 、 get

}
