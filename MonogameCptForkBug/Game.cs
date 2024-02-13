// - 1 -
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonogameCptForkBug
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.IsFullScreen = false;
        }

        private const int INDEX = 15;

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Effect _bugEffect;
        private VertexDeclaration _customVertex = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Single, VertexElementUsage.BlendIndices, 0));
        
        struct VertexPositionId
        {
            public Vector4 Position;
            public int Id;
        }

        private Matrix _view = Matrix.CreateLookAt(new Vector3(0, 0, -5), Vector3.Forward, Vector3.Up);
        private Matrix _proj;

        private RasterizerState _rs = new RasterizerState()
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.None
        };

        private StructuredBuffer _buffer1, _buffer2;

        protected override void Initialize()
        {
            base.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _bugEffect = Content.Load<Effect>("bug");

            VertexPositionId[] vertices = [new VertexPositionId() { Id = INDEX, Position = new Vector4(0, 0, 0, 1) }, new VertexPositionId() { Id = INDEX, Position = new Vector4(1, 0, 0, 1) }, new VertexPositionId() { Id = INDEX, Position = new Vector4(1, 1, 0, 1) }, new VertexPositionId() { Id = INDEX, Position = new Vector4(0, 1, 0, 1) }];

            _vertexBuffer = new VertexBuffer(GraphicsDevice, _customVertex, 4, BufferUsage.None);
            _vertexBuffer.SetData(vertices);

            _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.None);
            _indexBuffer.SetData([0, 1, 2, 2, 3, 0]);

            _proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000.0f);

            _buffer1 = new StructuredBuffer(GraphicsDevice, typeof(int), 1000, BufferUsage.None, ShaderAccess.ReadWrite);
            _buffer2 = new StructuredBuffer(GraphicsDevice, typeof(int), 1000, BufferUsage.None, ShaderAccess.ReadWrite);

            _buffer1.SetData(new int[1000]);
            _buffer1.SetData(new int[1000]);


            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            base.Update(gameTime);
        }

     

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = _rs;
            GraphicsDevice.Indices = _indexBuffer;
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);

            _bugEffect.Parameters["WorldViewProjection"].SetValue(_view * _proj);
            
            _bugEffect.Parameters["buffer1"].SetValue(_buffer1);
            //_bugEffect.Parameters["buffer2"].SetValue(_buffer2); 

            _bugEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);


            int[] results = new int[1000];
            _buffer1.GetData(results);

            var value = results[INDEX]; // must be 1000 (for some reason the value is correct only after the second frame).
                                        // .. now the bug in question: when assigning buffer2 too and writing to it in the pixel shader
                                        // the value wont be set at all..!


            base.Draw(gameTime); // set a breakpoint here.
        }

    }
}
