using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// todays endnote player position is not updatting globally, it is only updating in its own method only, fix that, can be fixed by craeting a m,ethod to update the position rather than having it in the event
/// Edit even that isnt working find another way
/// </summary>
namespace DirectionalShooter
{
    class Program
    {
        static void Main(string[] args)
        {
            MainFrame fr = new MainFrame();
        }
        class MainFrame : Form
        {
            Pen playerPen = new Pen(Color.Yellow, 7);//used to draw player
            Pen enemyPen = new Pen(Color.Red, 7);//used to draw enemy
            Pen bulletPen = new Pen(Color.Blue, 10);//used to draw bullets


            Graphics mainBox;//draws player box
            Graphics enemyBox;//draws enemy box
            Graphics bulltes;//draws bulltes

            string facing="DOWN";//which dir the player is facing, Initially faces down

            bool moving= false;//if the player is moving, initially false
            bool firing= false;//if the player is firing, initially false
            bool dashing = false;//if the player is dashing

            bool enemyIsAlive=false;//check is enemy is alive or requires to be respawned
            bool isPlayerAlive = true;//checks if the player has died.

            //position initialization

            int plPosX=50;//the top left x co-ord of the rectangle, initially 50
            int plPosY=50;//the top left y co-ord of the rectangle, initially 50
            int plWidth = 40;//the players width
            int plHeight = 40;//player height

            int enemyPosX;
            int enemyPosY;
            int enemyWidth = 40;
            int enemyHeight = 40;

            int bullPosX;
            int bullPosY;

            //Health stuff

            int playerHealth = 5;
            int enemyHealth = 2;

            string pressedKey = null;

            //the rectangles

            Rectangle player;
            Rectangle enemy;

            //theory
            //if x is the rectangles left most x coord then x+40 is its rightmost coord
            //same with y
            //if we check this to determine if they intersect it may work
            //if x or x+40 of player enters the area between x or x+40 of enemy AND if y or y+40 of player enters the area between y or y+40, they are basically intersecting


            int score = 0;


            TextBox debugBox = new TextBox();//need to initialize these components up here to be able to access in every method
            Panel screen = new Panel();//same, player is going to be on this panel as refreshing it wont harm other elements
            TextBox scoreBoard = new TextBox();//need to initialize these components up here to be able to access in every method
            TextBox healthBox = new TextBox();



            public MainFrame()
            {
                this.SetBounds(100,50,500,500);
                this.BackColor = Color.Black;
                this.KeyPreview = true;
                this.FormBorderStyle = FormBorderStyle.FixedSingle;//disables resizing which is a form border style and we set it to fixed single
                this.KeyDown += new KeyEventHandler(basic);//casting our void method as a keyEventHandler and adding it to event chain, since the return type of the event is keyEventHandler and not void

                screen.SetBounds(0,0, 500, 500);
                screen.BackColor = Color.Transparent;//setting the player layer as transparent as we dont want to hamper the other layers


                screen.Paint += new PaintEventHandler(initializePlayer);//this event initially paints the graphic onto the screen and initializes the player, it requires same as all casting to paint eventhandler and takes sender and event parameters which are in the referenced method below
                screen.Paint += new PaintEventHandler(initializeEnemy);//same as above, but we set it on enemylayer

                debugBox.SetBounds(10, 430, 460, 30);
                debugBox.Text = "Debug Screen";
                debugBox.ReadOnly = true;
                debugBox.Enabled = false;
                debugBox.TextAlign = HorizontalAlignment.Center;//align the text horizontally to the centre

                scoreBoard.SetBounds(400, 10, 70, 50);
                scoreBoard.Text = "0";
                scoreBoard.ReadOnly = true;
                scoreBoard.Enabled = false;
                scoreBoard.BackColor = Color.Black;
                scoreBoard.Font = new Font("Arial", 15);
                scoreBoard.ForeColor = Color.White;
                scoreBoard.TextAlign = HorizontalAlignment.Center;//align the text horizontally to the centre

                healthBox.SetBounds(50, 10, 70, 50);
                healthBox.Text = "0";
                healthBox.ReadOnly = true;
                healthBox.Enabled = false;
                healthBox.BackColor = Color.Black;
                healthBox.Font = new Font("Arial", 15);
                healthBox.ForeColor = Color.White;
                healthBox.TextAlign = HorizontalAlignment.Center;//align the text horizontally to the centre

                enemyPosSetter();//sets enemy position initially and if they die, also sets enemy alive to true, this is to set the position initially

                mainBox = screen.CreateGraphics();//initializing the player box by placing it into the screen panel and displaying it
                enemyBox = screen.CreateGraphics();//initiales enemy graphic and craetes it on enemylayer
                bulltes = screen.CreateGraphics();

                screen.SuspendLayout();
                this.SuspendLayout();
                this.Controls.Add(debugBox);
                this.Controls.Add(scoreBoard);
                this.Controls.Add(healthBox);
                this.Controls.Add(screen);
                this.Show();
                Application.Run(this);
            }

            public void basic(object Sender, KeyEventArgs e)//we take in event which will provide us with the event  of keyboard strokes and the sender
            {

                player = new Rectangle(plPosX, plPosY, plWidth, plHeight);//we need ot initialize the rectangle from here in the event
                positionUpdater(e.KeyCode.ToString());
                //enemyHealthSetter();//checking and damagingg and killing and everything else related to enemies health, this needs to be called from here as the position is updating only within this event and not globally
                //it is set withing the dashing method and firing method as the damage needs to be initializeed there
                scoreUpdater();

            }

            void initializePlayer(object sender, EventArgs e)
            {
                player = new Rectangle(plPosX, plPosY, plWidth, plHeight);//just to draw the initial rectangle using paint

                mainBox.DrawRectangle(playerPen, player);//can be drawn with a pen and a rectangle object
            }

            void initializeEnemy(object sender, EventArgs e)
            {
                enemy = new Rectangle(enemyPosX, enemyPosY, enemyWidth, enemyHeight);//we can crate and initialize the rectangle objects so they are easy to work with, here we initialize it so it can be drawn with paint fn
                enemyBox.DrawRectangle(enemyPen, enemy);
            }

            void enemyPosSetter()
            {
                if(enemyIsAlive!= true)
                {
                    enemyPosX = new Random().Next(400);//generates random value from 0 to 450
                    enemyPosY = new Random().Next(400);//generates random value from 0 to 450
                    enemyIsAlive = true;
                    enemyHealth = 2;//resets the enemies health
                }
                
            }

            void damageEnemy()
            {
                bool thresX1 = plPosX >= enemyPosX && plPosX <= enemyPosX + 40;//if the players Leftmost x coord gets inside the enemies x coord
                bool thresX2 = plPosX+40 >= enemyPosX && plPosX+40 <= enemyPosX + 40;//if the players Rightmost x coord gets inside the enemies x coord
                bool thresY1 = plPosY >= enemyPosY && plPosY <= enemyPosY + 40;//if the players Topmost y coord gets inside the enemies y coord
                bool thresY2 = plPosY+40 >= enemyPosY && plPosY+40 <= enemyPosY + 40;//if the players Topmost y coord gets inside the enemies y coord

                if ((thresX1 || thresX2) && (thresY1 || thresY2) && dashing) //checks if dashing player and enemy have collided
                {
                    enemyHealth--;
                    if (enemyHealth == 0)
                    {
                        score++;
                        enemyIsAlive = false;
                    }
                }
                if ((bullPosX>= enemyPosX && bullPosX<= enemyPosX + 40) && (bullPosY >= enemyPosY && bullPosY <= enemyPosY + 40)) //checks if the bullet and enemy have collided
                {
                    enemyHealth--;
                    if (enemyHealth == 0)
                    {
                        score++;
                        enemyIsAlive = false;
                    }
                }

                enemyPosSetter();//updates the enemies position when they die, cuz they only die within this event chain, this is to set it when they die
                debugBox.Text = $"players X-position: {plPosX}, players Y-position: {plPosY}, en X:{enemyPosX}, en Y: {enemyPosY}, Enemy alive: {enemyIsAlive}";


            }
            void positionUpdater(string key)
            {
                if (key=="W" && plPosY>0)//doesnt allow the plyr to go above 0
                {
                    screen.Invalidate();//clears and refreshes the screen, beware that since it refreshes the screen the other elements in the screen will also be refreshed.
                    plPosY -= 5;
                    moving = true;
                    facing = "UP";
                    damagePlayer();
                }
                if (key == "S" && plPosY < 420)//doesnbt allow to go below 420, which is the area till which square is seen
                {
                    screen.Invalidate();//clears and refreshes the screen
                    plPosY += 5;
                    moving = true;
                    facing = "DOWN";
                    damagePlayer();
                }
                if (key == "A" && plPosX > 0)//doesnt allow to go besides 0
                {
                    screen.Invalidate();//clears and refreshes the screen
                    plPosX -= 5;
                    moving = true;
                    facing = "LEFT";
                    damagePlayer();
                }
                if (key == "D" && plPosX < 440)//doesnt allow to go besides 440, which is the area till which square is seen
                {
                    screen.Invalidate();//clears and refreshes the screen
                    plPosX += 5;
                    moving = true;
                    facing = "RIGHT";
                    damagePlayer();//player also needs to be checked if they have touched enemy
                }
                if (key == "Space")
                {
                    if (facing.Equals("DOWN") && !dashing) {
                        dashing = true;
                        screen.Invalidate();//clears and refreshes the screen
                        plPosY += 100;
                        moving = true;
                        damageEnemy();//call to check if enemy hit during dash
                        //no need to set facing=down as they are already facing down
                        dashing = false;
                    }
                    if (facing.Equals("UP") && !dashing)
                    {
                        dashing = true;
                        screen.Invalidate();//clears and refreshes the screen
                        plPosY -= 100;
                        moving = true;
                        damageEnemy();
                        dashing = false;
                    }
                    if (facing.Equals("LEFT") && !dashing)
                    {
                        dashing = true;
                        screen.Invalidate();//clears and refreshes the screen
                        plPosX -= 100;
                        moving = true;
                        damageEnemy();
                        dashing = false;
                    }
                    if (facing.Equals("RIGHT") && !dashing)
                    {
                        dashing = true;
                        screen.Invalidate();//clears and refreshes the screen
                        plPosX += 100;
                        moving = true;
                        damageEnemy();
                        dashing = false;
                    }
                }

                if (key == "E")
                {
                    fireBullet();
                }

                mainBox.DrawRectangle(playerPen, player);
                //boxArrayCalculatorPlayer();//creates the array of point that a player covers, needs to be initialized from this array chain

            }

            void fireBullet()
            {
                bullPosX = (plPosX + (plPosX + 40)) / 2;//bullet starts from centre of players x axis
                bullPosY= (plPosY + (plPosY + 40)) / 2;//bullet starts from centre of players y axis
                firing = true;
                    if (facing.Equals("UP") && bullPosY > 0)
                    {
                        while (bullPosY > 0)
                        {
                            screen.Invalidate();
                            bulltes.DrawLine(bulletPen, bullPosX, bullPosY, bullPosX, --bullPosY);
                            bullPosY--;
                            damageEnemy();
                        }

                    }
                    if (facing.Equals("DOWN") && bullPosY < 420)
                    {
                        while (bullPosY < 420)
                        {
                            screen.Invalidate();
                            bulltes.DrawLine(bulletPen, bullPosX, bullPosY, bullPosX, ++bullPosY);
                            bullPosY++;
                            damageEnemy();
                        }

                    }
                    if (facing.Equals("LEFT") && bullPosX > 0)
                    {
                        while (bullPosX > 0)
                        {
                            screen.Invalidate();
                            bulltes.DrawLine(bulletPen, bullPosX, bullPosY, --bullPosX, bullPosY);
                            bullPosX--;
                            damageEnemy();
                        }

                    }
                    
                    if (facing.Equals("RIGHT") && bullPosX < 440 )
                    {
                        while (bullPosX < 440)
                        {
                            screen.Invalidate();
                            bulltes.DrawLine(bulletPen, bullPosX, bullPosY, ++bullPosX, bullPosY);
                            bullPosX++;
                            damageEnemy();
                        }
                    }
            }
            void damagePlayer()
            {
                bool thresX1 = plPosX >= enemyPosX && plPosX <= enemyPosX + 40;//if the players Leftmost x coord gets inside the enemies x coord
                bool thresX2 = plPosX + 40 >= enemyPosX && plPosX + 40 <= enemyPosX + 40;//if the players Rightmost x coord gets inside the enemies x coord
                bool thresY1 = plPosY >= enemyPosY && plPosY <= enemyPosY + 40;//if the players Topmost y coord gets inside the enemies y coord
                bool thresY2 = plPosY + 40 >= enemyPosY && plPosY + 40 <= enemyPosY + 40;//if the players Topmost y coord gets inside the enemies y coord

                if ((thresX1 || thresX2) && (thresY1 || thresY2)) //checks if dashing player and enemy have collided
                {
                    playerHealth--;
                    if (playerHealth == 0)
                    {
                        score=0;
                        playerHealth = 5;
                    }
                }
                healthBox.Text = "H: "+playerHealth.ToString();
            }

             void scoreUpdater()
            {
                scoreBoard.Text = "Sc: "+score.ToString();
            }

        }
    }
}
